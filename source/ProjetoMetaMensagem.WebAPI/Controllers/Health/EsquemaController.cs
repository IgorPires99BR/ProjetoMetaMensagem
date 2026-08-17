using Dapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProjetoMetaMensagem.Data;
using ProjetoMetaMensagem.WebAPI.Common;

namespace ProjetoMetaMensagem.WebAPI.Controllers.Health
{
    // Responde se o banco desta instancia esta com todas as migrations do BD/ aplicadas.
    //
    // Existe porque nao ha controle de migrations no projeto: quando uma tabela ou coluna
    // ficava pra tras em producao, o sintoma aparecia longe da causa. Os caminhos de leitura
    // usam SELECT *, que ignora coluna faltante sem erro, entao a lista continuava carregando
    // normalmente e so o INSERT/UPDATE que nomeia a coluna quebrava -- as vezes engolido, como
    // no webhook de status, que responde 200 pra Meta mesmo tendo falhado por dentro.
    //
    // Fica num controller separado de proposito: o HealthController e [AllowAnonymous], e
    // AllowAnonymous no controller vence [Authorize] posto na action -- este endpoint acabaria
    // publico, entregando o mapa do banco pra qualquer um.
    [ApiController]
    [Authorize]
    public class EsquemaController : ControllerBase
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<EsquemaController> _logger;
        private readonly IConfiguration _configuration;

        // Mesmo motivo do HealthController: DbSession abre a conexao no construtor, entao
        // injetar direto faria a excecao estourar na resolucao de DI, antes do try/catch.
        public EsquemaController(IServiceProvider serviceProvider, ILogger<EsquemaController> logger, IConfiguration configuration)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
            _configuration = configuration;
        }

        // Diz quais configuracoes criticas EXISTEM no ambiente, nunca o valor delas.
        //
        // Existe por causa de um caso real: o AppSecret nao estava configurado em producao, o
        // middleware do webhook descartava todo payload da Meta e respondia 200, e a plataforma
        // passou 5 dias sem receber uma unica mensagem sem que nada parecesse quebrado. De fora
        // nao havia como distinguir "configurado e correto" de "ausente". Agora ha.
        [HttpGet("api/health/configuracao")]
        public IActionResult Configuracao()
        {
            if (!this.EhAdminDaPlataforma())
            {
                return StatusCode(403, new { mensagem = "Acesso restrito à operação da plataforma.", tipo = "Negocio" });
            }

            // So o booleano. Devolver tamanho ou prefixo ja ajudaria quem tentasse adivinhar.
            bool Existe(string chave) => !string.IsNullOrWhiteSpace(_configuration[chave]);

            var itens = new[]
            {
                new { chave = "ApiWhatsappConnectionConfiguration:AppSecret", configurado = Existe("ApiWhatsappConnectionConfiguration:AppSecret"), usadoPara = "Validar a assinatura do webhook da Meta. Sem isto, nenhuma mensagem recebida entra." },
                new { chave = "ApiWhatsappConnectionConfiguration:AccessToken", configurado = Existe("ApiWhatsappConnectionConfiguration:AccessToken"), usadoPara = "Fallback de envio quando a empresa nao tem token proprio." },
                new { chave = "ApiWhatsappConnectionConfiguration:BaseUrl", configurado = Existe("ApiWhatsappConnectionConfiguration:BaseUrl"), usadoPara = "Endereco da Graph API da Meta." },
                new { chave = "JwtSettings:SecretKey", configurado = Existe("JwtSettings:SecretKey"), usadoPara = "Assinar o token de login. Sem isto a API nem sobe." },
                new { chave = "GeminiConfiguration:ApiKey", configurado = Existe("GeminiConfiguration:ApiKey"), usadoPara = "Assistente de IA. Sem isto so o assistente para de funcionar." },
                new { chave = "GmailConfig:SenhaApp", configurado = Existe("GmailConfig:SenhaApp"), usadoPara = "Envio de e-mail do sistema, como recuperacao de senha e acesso de cliente novo." },
                new { chave = "CaktoConfiguration:WebhookSecret", configurado = Existe("CaktoConfiguration:WebhookSecret"), usadoPara = "Validar o webhook de pagamento da Cakto. Sem isto, nenhuma conta de cliente novo e criada." },
            };

            var faltando = itens.Where(i => !i.configurado).Select(i => i.chave).ToList();

            return Ok(new
            {
                tudoConfigurado = faltando.Count == 0,
                faltando,
                itens,
                ambiente = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "desconhecido",
                horaServidor = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")
            });
        }

        private class ObjetoDoBanco
        {
            public string Tabela { get; set; } = string.Empty;
            public string? Coluna { get; set; }
        }

        [HttpGet("api/health/schema")]
        public async Task<IActionResult> Verificar()
        {
            // O mapa do banco so interessa a quem opera a plataforma, e nao ao admin do cliente.
            if (!this.EhAdminDaPlataforma())
            {
                return StatusCode(403, new { mensagem = "Acesso restrito à operação da plataforma.", tipo = "Negocio" });
            }

            List<ObjetoDoBanco> existentes;
            try
            {
                var session = _serviceProvider.GetRequiredService<DbSession>();

                // Uma consulta so: comparar em memoria sai mais barato que um EXISTS por item.
                var sql = @"
                    SELECT t.name AS Tabela, c.name AS Coluna
                    FROM sys.tables t
                    LEFT JOIN sys.columns c ON c.object_id = t.object_id";

                existentes = (await session.Connection.QueryAsync<ObjetoDoBanco>(sql)).ToList();
            }
            catch (Exception ex)
            {
                // Detalhe (servidor, base, credencial) fica so no log.
                _logger.LogError(ex, "Diagnostico de schema: falha ao consultar o banco");
                return StatusCode(503, new { banco = "indisponivel", tipo = "Servico" });
            }

            var tabelas = new HashSet<string>(existentes.Select(e => e.Tabela), StringComparer.OrdinalIgnoreCase);
            var colunas = new HashSet<string>(
                existentes.Where(e => e.Coluna != null).Select(e => $"{e.Tabela}.{e.Coluna}"),
                StringComparer.OrdinalIgnoreCase);

            bool Existe(EsquemaEsperado.Item item) => item.Coluna == null
                ? tabelas.Contains(item.Tabela)
                : colunas.Contains($"{item.Tabela}.{item.Coluna}");

            var porMigration = EsquemaEsperado.Itens
                .GroupBy(i => i.Migration)
                .OrderBy(g => g.Key, StringComparer.Ordinal)
                .Select(g =>
                {
                    var faltando = g.Where(i => !Existe(i))
                        .Select(i => i.Coluna == null ? i.Tabela : $"{i.Tabela}.{i.Coluna}")
                        .ToList();

                    return new
                    {
                        migration = g.Key,
                        estado = faltando.Count == 0 ? "ok" : "FALTANDO",
                        faltando
                    };
                })
                .ToList();

            var pendentes = porMigration.Where(m => m.estado != "ok").Select(m => m.migration).ToList();

            return Ok(new
            {
                banco = "ok",
                tudoAplicado = pendentes.Count == 0,
                pendentes,
                migrations = porMigration,
                ambiente = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "desconhecido",
                horaServidor = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")
            });
        }
    }
}
