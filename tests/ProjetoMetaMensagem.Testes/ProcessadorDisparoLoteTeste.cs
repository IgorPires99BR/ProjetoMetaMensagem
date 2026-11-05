using Microsoft.Extensions.Logging.Abstractions;
using Newtonsoft.Json;
using ProjetoMetaMensagem.Dominio.Entidades;
using ProjetoMetaMensagem.Dominio.Interfaces;
using ProjetoMetaMensagem.Dominio.Interfaces.Repositorios;
using ProjetoMetaMensagem.Dominio.Interfaces.Servicos;
using ProjetoMetaMensagem.Dominio.Interfaces.Servicos.Meta;
using ProjetoMetaMensagem.Dominio.Servicos;
using ProjetoMetaMensagem.Dominio.UseCases.Messages.EnviarMensagemTemplateMetaLote;
using System.Net.Http;
using System.Net.Sockets;
using System.Reflection;
using Xunit;

namespace ProjetoMetaMensagem.Testes;

// A regra que importa: o que a Meta pode ter recebido nunca e reenviado sozinho, e mensagem aceita
// nunca fica sem cobranca registrada.
public class ProcessadorDisparoLoteTeste
{
    [Fact]
    public async Task Envio_aceito_grava_historico_e_cobranca_juntos_e_limpa_o_Pix_do_item()
    {
        var c = new Cenario();

        await c.Processador.ProcessarProximoAsync();

        Assert.Equal(StatusDisparoItem.Enviado, c.Item.Status);
        Assert.Single(c.Historicos);
        var cobranca = Assert.Single(c.Cobrancas);
        Assert.Equal(c.Historicos[0].Id, cobranca.HistoricoDisparoId);
        Assert.Equal(1, c.Commits);
        Assert.Null(c.Item.EnvioJson);
        Assert.Null(c.Item.CobrancaJson);
    }

    [Fact]
    public async Task Situacao_e_gravada_como_enviando_antes_de_chamar_a_Meta()
    {
        var c = new Cenario();
        string? statusNaChamada = null;
        string? cobrancaNaChamada = null;
        c.AoEnviar = _ => { statusNaChamada = c.Item.Status; cobrancaNaChamada = c.Item.CobrancaJson; return Aceito(); };

        await c.Processador.ProcessarProximoAsync();

        Assert.Equal(StatusDisparoItem.Enviando, statusNaChamada);
        Assert.NotNull(cobrancaNaChamada);
    }

    [Fact]
    public async Task Limite_da_Meta_volta_para_a_fila_e_a_nova_tentativa_reaproveita_o_mesmo_Pix()
    {
        var c = new Cenario();
        var chamadas = 0;
        c.AoEnviar = _ => ++chamadas == 1 ? Falha(TipoFalhaEnvio.Transitoria) : Aceito();

        var primeiro = await c.Processador.ProcessarProximoAsync();

        Assert.Equal(ResultadoProcessamentoDisparo.Instavel, primeiro);
        Assert.Equal(StatusDisparoItem.Pendente, c.Item.Status);
        Assert.Empty(c.Cobrancas);
        Assert.Empty(c.Pix.Descartados);

        await c.Processador.ProcessarProximoAsync();

        Assert.Equal(StatusDisparoItem.Enviado, c.Item.Status);
        Assert.Equal(1, c.Pix.Preparacoes);
        Assert.Single(c.Cobrancas);
    }

    [Fact]
    public async Task Timeout_nao_reenvia_e_registra_a_cobranca_porque_o_cliente_pode_ter_recebido()
    {
        var c = new Cenario();
        var chamadas = 0;
        c.AoEnviar = _ => { chamadas++; return Falha(TipoFalhaEnvio.Incerta); };

        await c.Processador.ProcessarProximoAsync();
        var seguinte = await c.Processador.ProcessarProximoAsync();

        Assert.Equal(StatusDisparoItem.Incerto, c.Item.Status);
        Assert.Equal(ResultadoProcessamentoDisparo.SemItem, seguinte);
        Assert.Equal(1, chamadas);
        Assert.Single(c.Cobrancas);
        Assert.Empty(c.Pix.Descartados);
        Assert.NotNull(c.Historicos.Single().MotivoFalha);
    }

    [Fact]
    public async Task Recusa_definitiva_nao_repete_e_cancela_o_Pix_que_ninguem_recebeu()
    {
        var c = new Cenario();
        c.AoEnviar = _ => new ResultadoEnvioTemplate
        {
            Sucesso = false,
            TipoFalha = TipoFalhaEnvio.Definitiva,
            Erro = "{\"error\":{\"message\":\"Numero invalido\"}}"
        };

        await c.Processador.ProcessarProximoAsync();

        Assert.Equal(StatusDisparoItem.Falhou, c.Item.Status);
        Assert.Contains("Numero invalido", c.Item.Erro);
        Assert.DoesNotContain("{", c.Item.Erro);
        Assert.Empty(c.Cobrancas);
        Assert.Single(c.Pix.Descartados);
    }

    [Fact]
    public async Task Limite_que_persiste_desiste_depois_do_maximo_de_tentativas_e_cancela_o_Pix()
    {
        var c = new Cenario();
        var chamadas = 0;
        c.AoEnviar = _ => { chamadas++; return Falha(TipoFalhaEnvio.Transitoria); };

        for (var i = 0; i < 10; i++) await c.Processador.ProcessarProximoAsync();

        Assert.Equal(ProcessadorDisparoLote.MaximoDeTentativas, chamadas);
        Assert.Equal(StatusDisparoItem.Falhou, c.Item.Status);
        Assert.Single(c.Pix.Descartados);
    }

    [Fact]
    public async Task Queda_durante_o_envio_vira_incerto_e_ainda_registra_a_cobranca()
    {
        var c = new Cenario();
        c.AoEnviar = _ => throw new InvalidOperationException("processo caiu");

        await c.Processador.ProcessarProximoAsync();
        Assert.Equal(StatusDisparoItem.Enviando, c.Item.Status);
        Assert.Empty(c.Cobrancas);

        await c.Processador.ResolverInterrompidosAsync();

        Assert.Equal(StatusDisparoItem.Incerto, c.Item.Status);
        Assert.Single(c.Cobrancas);
        Assert.Single(c.Historicos);
    }

    [Fact]
    public async Task Falha_ao_gravar_depois_de_a_Meta_aceitar_nao_reenvia_e_a_recuperacao_registra()
    {
        var c = new Cenario();
        var chamadas = 0;
        c.AoEnviar = _ => { chamadas++; return Aceito(); };
        c.FalharAoGravarHistorico = true;

        await c.Processador.ProcessarProximoAsync();

        Assert.Equal(1, c.Rollbacks);
        Assert.Equal(StatusDisparoItem.Enviando, c.Item.Status);

        c.FalharAoGravarHistorico = false;
        await c.Processador.ResolverInterrompidosAsync();

        Assert.Equal(1, chamadas);
        Assert.Equal(StatusDisparoItem.Incerto, c.Item.Status);
        Assert.Single(c.Cobrancas);
    }

    [Fact]
    public async Task Dois_numeros_no_contato_abrem_uma_cobranca_so_e_a_falha_do_segundo_vira_ressalva()
    {
        var c = new Cenario(telefone2: "5511888880000");
        c.AoEnviar = telefone => telefone == "5511999990000" ? Aceito() : Falha(TipoFalhaEnvio.Definitiva, "sem whatsapp");

        await c.Processador.ProcessarProximoAsync();

        Assert.Equal(StatusDisparoItem.Enviado, c.Item.Status);
        Assert.Contains("sem whatsapp", c.Item.Erro);
        Assert.Single(c.Cobrancas);
        Assert.Single(c.Historicos);
    }

    [Fact]
    public async Task Provedor_de_Pix_fora_tenta_de_novo_sem_chamar_a_Meta()
    {
        var c = new Cenario();
        c.Pix.ErroNaPreparacao = "Pix não gerado: Asaas fora";
        c.Pix.ErroTransitorio = true;
        var chamadas = 0;
        c.AoEnviar = _ => { chamadas++; return Aceito(); };

        await c.Processador.ProcessarProximoAsync();

        Assert.Equal(StatusDisparoItem.Pendente, c.Item.Status);
        Assert.Equal(0, chamadas);

        c.Pix.ErroNaPreparacao = null;
        await c.Processador.ProcessarProximoAsync();

        Assert.Equal(StatusDisparoItem.Enviado, c.Item.Status);
        Assert.Equal(1, chamadas);
    }

    [Fact]
    public async Task Dado_errado_no_contato_falha_sem_nova_tentativa()
    {
        var c = new Cenario();
        c.Pix.ErroNaPreparacao = "Pix não gerado: o cadastro do contato está sem valor da fatura";

        await c.Processador.ProcessarProximoAsync();

        Assert.Equal(StatusDisparoItem.Falhou, c.Item.Status);
        Assert.Contains("valor da fatura", c.Item.Erro);
    }

    [Fact]
    public async Task Template_inativado_depois_de_entrar_na_fila_nao_sai()
    {
        var c = new Cenario();
        c.Template.Ativo = false;
        var chamadas = 0;
        c.AoEnviar = _ => { chamadas++; return Aceito(); };

        await c.Processador.ProcessarProximoAsync();

        Assert.Equal(StatusDisparoItem.Falhou, c.Item.Status);
        Assert.Equal(0, chamadas);
        Assert.Equal(0, c.Pix.Preparacoes);
    }

    [Fact]
    public async Task Contato_inativado_depois_de_entrar_na_fila_nao_sai()
    {
        var c = new Cenario();
        c.Contato.Ativo = false;

        await c.Processador.ProcessarProximoAsync();

        Assert.Equal(StatusDisparoItem.Falhou, c.Item.Status);
        Assert.Equal(0, c.Pix.Preparacoes);
    }

    [Theory]
    [InlineData(400, "{\"error\":{\"code\":130429}}", TipoFalhaEnvio.Transitoria)]
    [InlineData(400, "{\"error\":{\"code\":131056}}", TipoFalhaEnvio.Transitoria)]
    [InlineData(429, "", TipoFalhaEnvio.Transitoria)]
    [InlineData(503, "", TipoFalhaEnvio.Transitoria)]
    [InlineData(500, "{\"error\":{\"code\":131000}}", TipoFalhaEnvio.Incerta)]
    [InlineData(502, "", TipoFalhaEnvio.Incerta)]
    [InlineData(400, "{\"error\":{\"code\":131026}}", TipoFalhaEnvio.Definitiva)]
    [InlineData(401, "{\"error\":{\"code\":190}}", TipoFalhaEnvio.Definitiva)]
    [InlineData(400, "corpo que nao e json", TipoFalhaEnvio.Definitiva)]
    public void Resposta_da_Meta_e_classificada(int status, string corpo, TipoFalhaEnvio esperado)
    {
        Assert.Equal(esperado, ClassificadorFalhaMeta.DaResposta(status, corpo));
    }

    [Fact]
    public void So_falha_de_conexao_garante_que_a_mensagem_nao_saiu()
    {
        var recusada = new HttpRequestException("x", new SocketException((int)SocketError.ConnectionRefused));
        var caiuNoMeio = new HttpRequestException("x", new SocketException((int)SocketError.ConnectionReset));

        Assert.Equal(TipoFalhaEnvio.Transitoria, ClassificadorFalhaMeta.DaExcecao(recusada));
        Assert.Equal(TipoFalhaEnvio.Incerta, ClassificadorFalhaMeta.DaExcecao(caiuNoMeio));
        Assert.Equal(TipoFalhaEnvio.Incerta, ClassificadorFalhaMeta.DaExcecao(new TaskCanceledException()));
    }

    private static ResultadoEnvioTemplate Aceito() => new() { Sucesso = true, WamidMeta = "wamid.1" };

    private static ResultadoEnvioTemplate Falha(TipoFalhaEnvio tipo, string erro = "falhou") =>
        new() { Sucesso = false, TipoFalha = tipo, Erro = erro };

    private class Cenario
    {
        public DisparoLoteItem Item { get; }
        public Template Template { get; }
        public Contato Contato { get; }
        public PixFalso Pix { get; } = new();
        public List<HistoricoDisparo> Historicos { get; } = new();
        public List<CobrancaCliente> Cobrancas { get; } = new();
        public int Commits { get; set; }
        public int Rollbacks { get; set; }
        public bool FalharAoGravarHistorico { get; set; }
        public Func<string, ResultadoEnvioTemplate> AoEnviar { get; set; } = _ => Aceito();
        public ProcessadorDisparoLote Processador { get; }

        public Cenario(string? telefone2 = null)
        {
            var empresaId = Guid.NewGuid();
            Template = new Template { Id = Guid.NewGuid(), EmpresaId = empresaId, NomeTemplate = "cobranca", Conteudo = "Olá", Status = "APPROVED", GeraCobranca = true };
            Contato = new Contato { Id = Guid.NewGuid(), EmpresaId = empresaId, Telefone = "5511999990000", Telefone2 = telefone2, NomeContato = "Maria" };
            var lote = new DisparoLote { EmpresaId = empresaId, TemplateId = Template.Id, VariaveisJson = "[]" };
            Item = new DisparoLoteItem { LoteId = lote.Id, ContatoId = Contato.Id };

            var uow = Falso<IUnitOfWork>.Criar(new()
            {
                ["get_DisparoLote"] = _ => Falso<IDisparoLoteRepository>.Criar(new()
                {
                    ["ReservarProximo"] = _ => Task.FromResult<DisparoLoteItem?>(Reservar()),
                    ["ObterPorId"] = _ => Task.FromResult<DisparoLote?>(lote),
                    ["SalvarPreparacao"] = a => { Item.EnvioJson = (string)a[1]!; Item.CobrancaJson = (string?)a[2]; return Task.CompletedTask; },
                    ["MarcarEnviando"] = _ => { Item.Status = StatusDisparoItem.Enviando; return Task.FromResult(1); },
                    ["AgendarNovaTentativa"] = a => { Item.Status = StatusDisparoItem.Pendente; Item.Erro = (string?)a[2]; return Task.CompletedTask; },
                    ["Finalizar"] = a =>
                    {
                        Item.Status = (string)a[1]!; Item.Erro = (string?)a[2];
                        Item.EnvioJson = null; Item.CobrancaJson = null;
                        return Task.CompletedTask;
                    },
                    ["LiberarPreparacoesInterrompidas"] = _ => Task.FromResult(0),
                    ["ReservarEnvioInterrompido"] = _ => Task.FromResult<DisparoLoteItem?>(Item.Status == StatusDisparoItem.Enviando ? Item : null),
                }),
                ["get_Template"] = _ => Falso<ITemplateRepository>.Criar(new()
                {
                    ["ObterPorIdEEmpresa"] = _ => Task.FromResult<Template?>(Template)
                }),
                ["get_Contato"] = _ => Falso<IContatoRepository>.Criar(new()
                {
                    ["ObterPorIds"] = _ => Task.FromResult<IEnumerable<Contato>>(new[] { Contato })
                }),
                ["get_Empresa"] = _ => Falso<IEmpresaRepository>.Criar(new()
                {
                    ["ObterPhoneNumberId"] = _ => Task.FromResult<string?>("phone"),
                    ["ObterMetaAccessToken"] = _ => Task.FromResult<string?>("token")
                }),
                ["get_HistoricoDisparo"] = _ => Falso<IHistoricoDisparoRepository>.Criar(new()
                {
                    ["Incluir"] = a =>
                    {
                        if (FalharAoGravarHistorico) throw new InvalidOperationException("banco fora");
                        Historicos.Add((HistoricoDisparo)a[0]!);
                        return Task.CompletedTask;
                    }
                }),
                ["get_CobrancaCliente"] = _ => Falso<ICobrancaClienteRepository>.Criar(new()
                {
                    ["Incluir"] = a => { Cobrancas.Add((CobrancaCliente)a[0]!); return Task.CompletedTask; }
                }),
                ["BeginTransaction"] = _ => null,
                ["Commit"] = _ => { Commits++; return null; },
                ["Rollback"] = _ => { Rollbacks++; return null; },
            });

            var meta = Falso<IMetaService>.Criar(new()
            {
                ["EnviarTemplatesEmLoteAsync"] = a =>
                {
                    var envio = (EnviarMensagemTemplateMetaLoteCommand)a[0]!;
                    return Task.FromResult(envio.Telefones.Select((telefone, i) =>
                    {
                        var r = AoEnviar(telefone);
                        r.Telefone = telefone;
                        r.ContatoId = envio.ContatosIds[i];
                        return r;
                    }).ToList());
                }
            });

            Processador = new ProcessadorDisparoLote(uow, meta, Pix, NullLogger<ProcessadorDisparoLote>.Instance);
        }

        private DisparoLoteItem? Reservar()
        {
            if (Item.Status != StatusDisparoItem.Pendente) return null;
            Item.Status = StatusDisparoItem.Preparando;
            Item.Tentativas++;
            return Item;
        }
    }

    private class PixFalso : IPreparadorCobrancaPix
    {
        public int Preparacoes { get; private set; }
        public string? ErroNaPreparacao { get; set; }
        public bool ErroTransitorio { get; set; }
        public List<CobrancaPreparada> Descartados { get; } = new();

        public Task<Dictionary<Guid, CobrancaPreparada>> PrepararAsync(Guid empresaId, Template template, IEnumerable<Contato> contatos, DateTime agora)
        {
            Preparacoes++;
            var contato = contatos.Single();
            var cobranca = new CobrancaCliente { EmpresaId = empresaId, ContatoId = contato.Id, TemplateId = template.Id, Valor = 100m };
            if (ErroNaPreparacao == null) { cobranca.Txid = "txid"; cobranca.PixIdExterno = "pay"; }
            return Task.FromResult(new Dictionary<Guid, CobrancaPreparada>
            {
                [contato.Id] = new() { Cobranca = cobranca, Erro = ErroNaPreparacao, ErroTransitorio = ErroTransitorio }
            });
        }

        public List<ResultadoEnvioTemplate> AplicarNoLote(EnviarMensagemTemplateMetaLoteCommand command, Template? template, Dictionary<Guid, CobrancaPreparada>? preparadas)
        {
            var erro = preparadas?.Values.FirstOrDefault()?.Erro;
            if (erro == null) return new();

            var falhas = command.Telefones.Select(t => new ResultadoEnvioTemplate { Erro = erro, Telefone = t }).ToList();
            command.Telefones = new();
            command.ContatosIds = new();
            return falhas;
        }

        public Task DescartarAsync(Guid empresaId, IEnumerable<CobrancaPreparada> naoEnviadas)
        {
            Descartados.AddRange(naoEnviadas);
            return Task.CompletedTask;
        }

        public Task<CobrancaPreparada> GarantirPixAsync(CobrancaCliente cobranca, Contato contato, DateTime agora) => throw new NotSupportedException();
        public Task<string?> EncerrarPixAsync(CobrancaCliente cobranca) => throw new NotSupportedException();
        public string? AplicarNasVariaveis(List<string> valores, CobrancaPreparada? preparada) => throw new NotSupportedException();
    }

    // As interfaces tem dezenas de membros e cada teste usa poucos: o que nao foi definido estoura,
    // em vez de responder um valor qualquer.
    private class Falso<T> : DispatchProxy where T : class
    {
        private Dictionary<string, Func<object?[], object?>> _membros = new();

        public static T Criar(Dictionary<string, Func<object?[], object?>> membros)
        {
            var proxy = Create<T, Falso<T>>();
            ((Falso<T>)(object)proxy)._membros = membros;
            return proxy;
        }

        protected override object? Invoke(MethodInfo? metodo, object?[]? argumentos) =>
            _membros.TryGetValue(metodo!.Name, out var membro)
                ? membro(argumentos ?? Array.Empty<object?>())
                : throw new NotSupportedException($"{typeof(T).Name}.{metodo.Name} não definido no teste.");
    }
}
