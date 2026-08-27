using Microsoft.Extensions.Configuration;
using ProjetoMetaMensagem.Data;
using ProjetoMetaMensagem.Data.Repositorios;
using ProjetoMetaMensagem.Dominio.Entidades;
using ProjetoMetaMensagem.Dominio.UseCases.Flows.ListaFlows;
using Xunit;

namespace ProjetoMetaMensagem.Testes;

// Estes testes existem por causa de uma familia de bug que ja apareceu duas vezes em producao:
// um campo novo de Flow e adicionado na entidade e na tela, mas alguem esquece de incluir no
// INSERT do repositorio ou no DTO de leitura. Nada disso quebra o build -- o campo simplesmente
// some, sem erro, e o sintoma aparece longe da causa.
//
//   1a vez: VariavelSaida existia e a tela mandava o valor, mas o INSERT nao listava a coluna.
//           Nenhuma etapa de captura guardava a resposta do cliente.
//   2a vez: Botao1/Botao2/ProximaEtapaIdB nao voltavam na leitura. Abrir um flow na tela e
//           salvar apagava os botoes e a ramificacao configurados.
//
// A regra que estes testes protegem: TODO campo de Flow/FlowEtapa tem que sobreviver a uma
// volta completa -- gravar, reler e continuar igual.
public class FlowIdaEVoltaTeste
{
    // O teste de banco usa o SQL local (mesma connection string do appsettings de
    // desenvolvimento). Sem banco no ar ele e ignorado em vez de falhar, pra nao dar alarme
    // falso em maquina que nao subiu o container.
    private const string ConexaoLocal =
        "Server=localhost,1433;Database=ContactSolutionDB;User Id=sa;Password=SuaSenhaForte123!;TrustServerCertificate=True;Connect Timeout=5";

    private static DbSession? AbrirSessao()
    {
        try
        {
            var config = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string?>
                {
                    ["ConnectionStrings:ContactSolutionDB"] = ConexaoLocal,
                    ["ConnectionStrings:ContactProdDB"] = ConexaoLocal,
                })
                .Build();

            return new DbSession(config);
        }
        catch
        {
            return null;
        }
    }

    private static async Task<Guid?> PrimeiraEmpresa(DbSession session)
    {
        var cmd = session.Connection.CreateCommand();
        cmd.CommandText = "SELECT TOP 1 Id FROM Empresa";
        var valor = cmd.ExecuteScalar();
        return await Task.FromResult(valor is Guid id ? id : (Guid?)null);
    }

    [Fact]
    public async Task Flow_gravado_com_todos_os_campos_volta_igual_do_banco()
    {
        using var session = AbrirSessao();
        if (session == null) return; // banco local indisponivel

        var repo = new FlowRepository(session);

        // Fluxo tem FK para Empresa, entao o teste usa uma empresa que ja exista no banco em
        // vez de inventar um id. Sem nenhuma empresa cadastrada nao ha o que testar.
        var empresaId = await PrimeiraEmpresa(session);
        if (empresaId == null) return;

        var flowId = Guid.NewGuid();
        var etapaPerguntaId = Guid.NewGuid();
        var etapaDestinoBId = Guid.NewGuid();

        var flow = new Flow
        {
            Id = flowId,
            EmpresaId = empresaId.Value,
            Nome = "Teste ida e volta",
            Descricao = "criado por teste automatizado",
            GatilhoInicial = "gatilho-teste,*",
            Ativo = true,
            DataCriacao = DateTime.Now,
            NumeroId = null,
            SourceIdAnuncio = "ANUNCIO-TESTE-9",
        };

        // Destino da ramificacao entra primeiro: ProximaEtapaId e uma FK auto-referenciada.
        var etapaDestinoB = new FlowEtapa
        {
            Id = etapaDestinoBId,
            FlowId = flowId,
            NomeEtapa = "Mensagem",
            ConteudoLivre = "Caminho B",
            GatilhoResposta = "Avancar",
            EhEtapaInicial = false,
        };

        var etapaPergunta = new FlowEtapa
        {
            Id = etapaPerguntaId,
            FlowId = flowId,
            NomeEtapa = "Capturar Input",
            ConteudoLivre = "Por onde comecar?",
            GatilhoResposta = "Qualquer_Resposta",
            EhEtapaInicial = true,
            VariavelSaida = "porta",
            Botao1 = "Opcao A",
            Botao2 = "Opcao B",
            ProximaEtapaIdB = etapaDestinoBId,
        };

        try
        {
            await repo.Incluir(flow);
            await repo.IncluirEtapa(etapaDestinoB);
            await repo.IncluirEtapa(etapaPergunta);

            var lidos = await repo.ObterTodosPorEmpresa(empresaId.Value);
            var flowLido = Assert.Single(lidos.Where(f => f.Id == flowId));

            Assert.Equal("gatilho-teste,*", flowLido.GatilhoInicial);
            Assert.Equal("ANUNCIO-TESTE-9", flowLido.SourceIdAnuncio);

            var perguntaLida = Assert.Single(flowLido.Etapas.Where(e => e.Id == etapaPerguntaId));
            Assert.Equal("porta", perguntaLida.VariavelSaida);
            Assert.Equal("Opcao A", perguntaLida.Botao1);
            Assert.Equal("Opcao B", perguntaLida.Botao2);
            Assert.Equal(etapaDestinoBId, perguntaLida.ProximaEtapaIdB);
        }
        finally
        {
            await repo.ExcluirEtapasPorFlowId(flowId, null);
            await repo.Excluir(flowId, null);
        }
    }

    [Fact]
    public void Leitura_do_flow_devolve_botoes_ramificacao_e_anuncio()
    {
        // Sem banco: o que se checa aqui e o mapeamento entidade -> DTO, que foi exatamente
        // onde os campos sumiram da tela na segunda vez.
        var etapaDestinoB = Guid.NewGuid();

        var flow = new Flow
        {
            Id = Guid.NewGuid(),
            EmpresaId = Guid.NewGuid(),
            Nome = "Flow",
            Descricao = "",
            GatilhoInicial = "oi",
            Ativo = true,
            SourceIdAnuncio = "ANUNCIO-42",
            Etapas = new List<FlowEtapa>
            {
                new()
                {
                    Id = Guid.NewGuid(),
                    NomeEtapa = "Capturar Input",
                    ConteudoLivre = "Qual plano?",
                    GatilhoResposta = "Qualquer_Resposta",
                    VariavelSaida = "plano",
                    Botao1 = "Starter",
                    Botao2 = "Pro",
                    ProximaEtapaIdB = etapaDestinoB,
                },
            },
        };

        var dto = new ListaFlowsResult(flow);
        var etapa = Assert.Single(dto.Etapas);

        Assert.Equal("ANUNCIO-42", dto.SourceIdAnuncio);
        Assert.Equal("plano", etapa.VariavelSaida);
        Assert.Equal("Starter", etapa.Botao1);
        Assert.Equal("Pro", etapa.Botao2);
        Assert.Equal(etapaDestinoB, etapa.ProximaEtapaIdB);
    }
}
