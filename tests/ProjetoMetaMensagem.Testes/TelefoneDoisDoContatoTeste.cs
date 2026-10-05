using ProjetoMetaMensagem.Dominio.UseCases.Contato.AlteraContato;
using ProjetoMetaMensagem.Dominio.UseCases.Contato.CriaContato;
using Xunit;

namespace ProjetoMetaMensagem.Testes;

// Contato com dois numeros (ex: empresa de dois socios, marido e mulher): o disparo sai para os
// dois, mas a cobranca e uma so. O que estes testes protegem: o cadastro, que e onde um segundo
// numero errado viraria mensagem duplicada no mesmo WhatsApp.
public class TelefoneDoisDoContatoTeste
{
    private static CriaContatoCommand Criacao(string telefone, string? telefone2) => new()
    {
        UsuarioId = Guid.NewGuid(),
        EmpresaId = Guid.NewGuid(),
        Telefone = telefone,
        Telefone2 = telefone2
    };

    private static AlteraContatoCommand Alteracao(string telefone, string? telefone2) => new()
    {
        Id = Guid.NewGuid(),
        UsuarioId = Guid.NewGuid(),
        EmpresaId = Guid.NewGuid(),
        Telefone = telefone,
        Telefone2 = telefone2
    };

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("5511988888888")]
    public void Aceita_contato_sem_ou_com_segundo_numero(string? telefone2)
    {
        Assert.True(new CriaContatoValidator().Validate(Criacao("5511999999999", telefone2)).IsValid);
        Assert.True(new AlteraContatoValidator().Validate(Alteracao("5511999999999", telefone2)).IsValid);
    }

    [Theory]
    [InlineData("5511999999999")]
    // Mesmo numero escrito sem o 55: a Meta entrega nos dois casos no mesmo WhatsApp, e o
    // cliente receberia a mensagem duas vezes.
    [InlineData("11999999999")]
    [InlineData("+55 (11) 99999-9999")]
    public void Recusa_segundo_numero_igual_ao_principal(string telefone2)
    {
        var criacao = new CriaContatoValidator().Validate(Criacao("5511999999999", telefone2));
        var alteracao = new AlteraContatoValidator().Validate(Alteracao("5511999999999", telefone2));

        Assert.Contains(criacao.Errors, e => e.ErrorMessage.Contains("telefone 2 não pode ser igual"));
        Assert.Contains(alteracao.Errors, e => e.ErrorMessage.Contains("telefone 2 não pode ser igual"));
    }

    [Fact]
    public void Telefone2_em_branco_vira_nulo_no_contato()
    {
        // Gravar "" faria o disparo tentar enviar para um numero vazio.
        Assert.Null(new Dominio.Entidades.Contato(Criacao("5511999999999", "   ")).Telefone2);
        Assert.Equal("5511988888888", new Dominio.Entidades.Contato(Alteracao("5511999999999", " 5511988888888 ")).Telefone2);
    }
}

// Checagem de duplicado na edicao: so o numero que mudou vai pro banco. Contatos que ja dividem
// numero (mesma pessoa respondendo por varias empresas do cliente) precisam continuar editaveis.
public class NumerosNovosNaEdicaoTeste
{
    private static Dominio.Entidades.Contato Atual(string telefone, string? telefone2) =>
        new() { Telefone = telefone, Telefone2 = telefone2 };

    [Fact]
    public void Editar_sem_mudar_numeros_nao_checa_nada()
    {
        Assert.Empty(AlteraContatoHandler.NumerosNovos(Atual("5511999999999", "5511988888888"), "5511999999999", "5511988888888"));
    }

    [Fact]
    public void Trocar_principal_com_telefone2_ou_so_a_formatacao_nao_e_numero_novo()
    {
        Assert.Empty(AlteraContatoHandler.NumerosNovos(Atual("5511999999999", "5511988888888"), "5511988888888", "+55 (11) 99999-9999"));
    }

    [Fact]
    public void Adicionar_telefone2_checa_so_ele()
    {
        Assert.Equal(new[] { "5511977777777" },
            AlteraContatoHandler.NumerosNovos(Atual("5511999999999", null), "5511999999999", " 5511977777777 "));
    }

    [Fact]
    public void Trocar_o_principal_checa_o_novo_principal()
    {
        Assert.Equal(new[] { "5511966666666" },
            AlteraContatoHandler.NumerosNovos(Atual("5511999999999", null), "5511966666666", ""));
    }
}
