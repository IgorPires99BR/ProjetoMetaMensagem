using Microsoft.Extensions.Configuration;
using ProjetoMetaMensagem.Data;
using ProjetoMetaMensagem.Data.Repositorios;
using ProjetoMetaMensagem.Dominio.UseCases.Usuario.CriaUsuario;
using ProjetoMetaMensagem.Dominio.UseCases.Usuario.TrocaSenha;
using Xunit;

namespace ProjetoMetaMensagem.Testes;

// O e-mail de boas-vindas manda o cliente "trocar essa senha no primeiro acesso", mas nao
// existia tela para isso: a unica saida era "Esqueci minha senha", que sorteia OUTRA senha e
// manda por e-mail. O cliente nunca escolhia a propria senha.
//
// O que estes testes protegem: as recusas que fazem a tela ser segura, e o fato de a troca
// mexer SO na senha -- se ela encostasse em IsAdmin, trocar a senha rebaixaria o dono da conta.
public class TrocaDeSenhaTeste
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

    private static TrocaSenhaCommand Comando(string atual, string nova, string? confirmacao = null) =>
        new()
        {
            SenhaAtual = atual,
            SenhaNova = nova,
            ConfirmacaoSenhaNova = confirmacao ?? nova,
        };

    [Theory]
    // A confirmacao existe justamente para pegar o erro de digitacao: sem ela, a pessoa
    // ficaria trancada fora de uma conta cuja senha ela digitou errado sem perceber.
    [InlineData("SenhaAtual1", "SenhaNova123", "OutraCoisa1", "confirmação")]
    [InlineData("SenhaAtual1", "abc", null, "6 caracteres")]
    // Trocar a senha pela mesma da a impressao de ter funcionado sem mudar nada -- e quem vem
    // do e-mail de boas-vindas esta tentando justamente sair da senha sorteada.
    [InlineData("SenhaAtual1", "SenhaAtual1", null, "diferente da atual")]
    [InlineData("", "SenhaNova123", null, "senha atual")]
    public void Recusa_o_que_precisa_ser_recusado(string atual, string nova, string? confirmacao, string trechoEsperado)
    {
        var resultado = new TrocaSenhaValidator().Validate(Comando(atual, nova, confirmacao));

        Assert.False(resultado.IsValid);
        Assert.Contains(resultado.Errors, e => e.ErrorMessage.Contains(trechoEsperado, StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public void Aceita_uma_troca_bem_formada()
    {
        Assert.True(new TrocaSenhaValidator().Validate(Comando("SenhaAtual1", "SenhaNova123")).IsValid);
    }

    [Fact]
    public async Task Trocar_a_senha_nao_mexe_em_mais_nada_do_usuario()
    {
        using var session = AbrirSessao();
        if (session == null) return;

        var empresaId = PrimeiraEmpresa(session);
        if (empresaId == null) return;

        var repo = new UsuarioRepository(session);
        var id = Guid.NewGuid();

        await repo.Incluir(new Dominio.Entidades.Usuario(new CriaUsuarioCommand
        {
            EmpresaId = empresaId.Value,
            Nome = "Dono da conta",
            Email = $"troca-{id:N}@teste.local",
            SenhaHash = BCrypt.Net.BCrypt.HashPassword("SenhaAtual1"),
            Perfil = "admin",
        })
        { Id = id });

        try
        {
            var linhas = await repo.AlterarSenha(id, BCrypt.Net.BCrypt.HashPassword("SenhaNova123"));
            Assert.Equal(1, linhas);

            var depois = await repo.ObterPorId(id);
            Assert.NotNull(depois);

            Assert.True(BCrypt.Net.BCrypt.Verify("SenhaNova123", depois!.SenhaHash), "A senha nova nao foi gravada.");
            Assert.False(BCrypt.Net.BCrypt.Verify("SenhaAtual1", depois.SenhaHash), "A senha antiga continua valendo.");

            // O ponto do teste: trocar a senha nao pode rebaixar quem trocou. Um UPDATE que
            // montasse o usuario inteiro a partir do comando zeraria isto sem avisar.
            Assert.True(depois.IsAdmin, "Trocar a senha rebaixou o admin.");
            Assert.Equal("Dono da conta", depois.Nome);
        }
        finally
        {
            await repo.Excluir(id.ToString(), empresaId);
        }
    }
}
