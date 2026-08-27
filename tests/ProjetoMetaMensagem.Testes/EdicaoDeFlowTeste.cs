using Microsoft.Extensions.Configuration;
using ProjetoMetaMensagem.Data;
using ProjetoMetaMensagem.Data.Repositorios;
using ProjetoMetaMensagem.Dominio.Entidades;
using Xunit;

namespace ProjetoMetaMensagem.Testes;

// Editar um flow apagava e recriava TODAS as etapas com Ids novos. Como a conversa em andamento
// guarda o Id da etapa onde parou, qualquer edicao deixaria esse ponteiro orfao -- e por isso
// existia a trava "este fluxo tem conversas em andamento e nao pode ser editado".
//
// Na pratica a trava custou caro: pra mexer no texto de uma etapa foi preciso encerrar 22
// conversas de leads a forca. E quanto melhor a campanha funciona, mais gente fica no meio de
// uma conversa e mais dificil fica editar.
//
// O que estes testes protegem: atualizar uma etapa NAO pode trocar o Id dela.
public class EdicaoDeFlowTeste
{
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

    private static Guid? PrimeiraEmpresa(DbSession session)
    {
        var cmd = session.Connection.CreateCommand();
        cmd.CommandText = "SELECT TOP 1 Id FROM Empresa";
        return cmd.ExecuteScalar() is Guid id ? id : null;
    }

    [Fact]
    public async Task Atualizar_etapa_muda_o_conteudo_e_preserva_o_Id()
    {
        using var session = AbrirSessao();
        if (session == null) return;

        var empresaId = PrimeiraEmpresa(session);
        if (empresaId == null) return;

        var repo = new FlowRepository(session);
        var flowId = Guid.NewGuid();
        var etapaId = Guid.NewGuid();

        await repo.Incluir(new Flow
        {
            Id = flowId,
            EmpresaId = empresaId.Value,
            Nome = "Teste edicao",
            Descricao = "",
            GatilhoInicial = "editar-teste",
            Ativo = true,
            DataCriacao = DateTime.Now,
        });

        await repo.IncluirEtapa(new FlowEtapa
        {
            Id = etapaId,
            FlowId = flowId,
            NomeEtapa = "Capturar Input",
            ConteudoLivre = "Texto antigo",
            GatilhoResposta = "Qualquer_Resposta",
            EhEtapaInicial = true,
            VariavelSaida = "nome",
            Botao1 = "Sim",
            Botao2 = "Nao",
        });

        try
        {
            await repo.AlterarEtapa(new FlowEtapa
            {
                Id = etapaId,
                FlowId = flowId,
                NomeEtapa = "Capturar Input",
                ConteudoLivre = "Texto NOVO",
                GatilhoResposta = "Qualquer_Resposta",
                EhEtapaInicial = true,
                VariavelSaida = "nome_completo",
                Botao1 = "Claro",
                Botao2 = "Agora nao",
            });

            var lida = await repo.ObterEtapaPorId(etapaId);

            Assert.NotNull(lida);
            // O Id continuar o mesmo e o ponto do teste: e ele que a conversa em andamento
            // guarda. Se mudar, a conversa fica apontando pra etapa que nao existe mais.
            Assert.Equal(etapaId, lida!.Id);
            Assert.Equal("Texto NOVO", lida.ConteudoLivre);
            Assert.Equal("nome_completo", lida.VariavelSaida);
            Assert.Equal("Claro", lida.Botao1);
            Assert.Equal("Agora nao", lida.Botao2);
        }
        finally
        {
            await repo.ExcluirEtapasPorFlowId(flowId, null);
            await repo.Excluir(flowId, null);
        }
    }

    [Fact]
    public async Task Excluir_uma_etapa_nao_derruba_as_outras()
    {
        using var session = AbrirSessao();
        if (session == null) return;

        var empresaId = PrimeiraEmpresa(session);
        if (empresaId == null) return;

        var repo = new FlowRepository(session);
        var flowId = Guid.NewGuid();
        var ficaId = Guid.NewGuid();
        var saiId = Guid.NewGuid();

        await repo.Incluir(new Flow
        {
            Id = flowId,
            EmpresaId = empresaId.Value,
            Nome = "Teste exclusao parcial",
            Descricao = "",
            GatilhoInicial = "excluir-teste",
            Ativo = true,
            DataCriacao = DateTime.Now,
        });

        await repo.IncluirEtapa(new FlowEtapa { Id = saiId, FlowId = flowId, NomeEtapa = "Mensagem", ConteudoLivre = "Vai sair", GatilhoResposta = "Avancar" });
        await repo.IncluirEtapa(new FlowEtapa { Id = ficaId, FlowId = flowId, NomeEtapa = "Mensagem", ConteudoLivre = "Vai ficar", GatilhoResposta = "Avancar", EhEtapaInicial = true });

        try
        {
            await repo.ExcluirEtapa(saiId);

            var restantes = await repo.ObterEtapasPorFlow(flowId);
            var restante = Assert.Single(restantes);

            Assert.Equal(ficaId, restante.Id);
            Assert.Equal("Vai ficar", restante.ConteudoLivre);
        }
        finally
        {
            await repo.ExcluirEtapasPorFlowId(flowId, null);
            await repo.Excluir(flowId, null);
        }
    }
}
