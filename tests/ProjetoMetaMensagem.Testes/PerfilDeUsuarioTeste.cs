using Microsoft.Extensions.Configuration;
using ProjetoMetaMensagem.Data;
using ProjetoMetaMensagem.Data.Repositorios;
using ProjetoMetaMensagem.Dominio.Entidades;
using ProjetoMetaMensagem.Dominio.UseCases.Usuario.AlteraUsuario;
using ProjetoMetaMensagem.Dominio.UseCases.Usuario.CriaUsuario;
using ProjetoMetaMensagem.Dominio.UseCases.Usuario.ObtemUsuario;
using Xunit;

namespace ProjetoMetaMensagem.Testes;

// A tela de Usuarios sempre mandou "perfil", mas CriaUsuarioCommand nao tinha o campo: o valor
// era descartado no bind do model e IsAdmin caia no default false. Resultado: escolher
// "Administrador (Total)" criava um operador, e a tela dizia "Usuario criado com sucesso".
// Na pratica o dono da conta nao conseguia dar acesso de admin a um socio -- o socio entrava e
// nao via Chats Ativos nem Flows, sem nenhuma mensagem explicando por que.
//
// Nada disso quebrava o build: as duas pontas estavam certas isoladamente e so o meio faltava.
public class PerfilDeUsuarioTeste
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
    public void Perfil_admin_vira_IsAdmin_verdadeiro_na_criacao()
    {
        var usuario = new Dominio.Entidades.Usuario(new CriaUsuarioCommand
        {
            EmpresaId = Guid.NewGuid(),
            Nome = "Socio",
            Email = "socio@exemplo.com",
            Perfil = "admin",
        });

        Assert.True(usuario.IsAdmin);
    }

    [Fact]
    public void Perfil_ausente_cai_em_operador_e_nao_em_admin()
    {
        var usuario = new Dominio.Entidades.Usuario(new CriaUsuarioCommand
        {
            EmpresaId = Guid.NewGuid(),
            Nome = "Sem perfil",
            Email = "semperfil@exemplo.com",
        });

        // O default precisa ser o MENOS privilegiado: quem nao pediu admin nao recebe admin.
        Assert.False(usuario.IsAdmin);
    }

    [Fact]
    public void Edicao_sem_perfil_nao_rebaixa_o_usuario()
    {
        var usuario = new Dominio.Entidades.Usuario(new AlteraUsuarioCommand
        {
            Id = Guid.NewGuid(),
            EmpresaId = Guid.NewGuid(),
            Nome = "So trocando o nome",
            Perfil = null,
        });

        // null aqui significa "nao mexer no perfil" -- o UPDATE usa COALESCE. Se virasse false,
        // uma redefinicao de senha tiraria o admin do dono da conta.
        Assert.Null(usuario.IsAdmin);
    }

    [Fact]
    public async Task Perfil_sobrevive_a_ida_e_volta_no_banco()
    {
        using var session = AbrirSessao();
        if (session == null) return;

        var empresaId = PrimeiraEmpresa(session);
        if (empresaId == null) return;

        var repo = new UsuarioRepository(session);
        var id = Guid.NewGuid();
        var email = $"perfil-{id:N}@teste.local";

        await repo.Incluir(new Dominio.Entidades.Usuario(new CriaUsuarioCommand
        {
            EmpresaId = empresaId.Value,
            Nome = "Admin de teste",
            Email = email,
            SenhaHash = "hash-qualquer",
            Perfil = "admin",
        })
        { Id = id });

        try
        {
            var lido = await repo.ObterPorId(id);
            Assert.NotNull(lido);
            Assert.True(lido!.IsAdmin, "Perfil admin nao chegou ao banco -- o campo se perdeu no caminho.");

            // A listagem tambem precisa devolver o perfil, senao a tela mostra a coluna vazia
            // e a edicao volta com o valor errado pre-selecionado.
            Assert.Equal(Dominio.Entidades.Usuario.PerfilAdmin, new ObtemUsuarioResult(lido).Perfil);

            // Uma edicao que nao fala de perfil (troca de senha, por exemplo) mantem o admin.
            lido.SenhaHash = "outro-hash";
            lido.IsAdmin = null;
            await repo.Alterar(lido, empresaId);

            var depois = await repo.ObterPorId(id);
            Assert.True(depois!.IsAdmin, "Uma edicao sem perfil rebaixou o admin.");
        }
        finally
        {
            await repo.Excluir(id.ToString(), empresaId);
        }
    }
}
