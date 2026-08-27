using ProjetoMetaMensagem.Dominio.UseCases.Empresa.CriaContaCliente;
using Xunit;

namespace ProjetoMetaMensagem.Testes;

// A conta de um cliente so nascia inteira pelo webhook de pagamento da Cakto. Cadastrar um
// cliente que fechou por fora exigia criar a empresa numa tela e o usuario admin dela direto no
// banco -- a tela de Usuarios sempre usa a empresa de quem esta logado, entao nao havia como
// apontar para a empresa recem criada.
//
// O que estes testes protegem: as bordas do formulario que a equipe usa todo dia. A tela filtra
// boa parte, mas a API tambem e chamada por fora dela.
public class CriacaoDeContaClienteTeste
{
    private static CriaContaClienteCommand Comando(
        string nome = "Padaria do Bairro",
        string email = "cliente@exemplo.com",
        string plano = "STARTER") =>
        new() { Nome = nome, Email = email, Plano = plano };

    [Theory]
    [InlineData("", "cliente@exemplo.com", "STARTER", "nome")]
    // Sem e-mail a conta nasce sem ninguem conseguir entrar nela: e o login do cliente e o
    // endereco por onde a senha chega.
    [InlineData("Padaria", "", "STARTER", "e-mail")]
    [InlineData("Padaria", "naoehemail", "STARTER", "válido")]
    [InlineData("Padaria", "cliente@exemplo.com", "", "plano")]
    public void Recusa_cadastro_sem_o_minimo(string nome, string email, string plano, string trechoEsperado)
    {
        var resultado = new CriaContaClienteValidator().Validate(Comando(nome, email, plano));

        Assert.False(resultado.IsValid);
        Assert.Contains(resultado.Errors, e => e.ErrorMessage.Contains(trechoEsperado, StringComparison.OrdinalIgnoreCase));
    }

    [Theory]
    [InlineData("STARTER")]
    [InlineData("PRO")]
    [InlineData("ENTERPRISE")]
    // A tela manda maiusculo, mas quem chama a API por fora escreve como quiser.
    [InlineData("starter")]
    public void Aceita_os_planos_que_existem(string plano)
    {
        Assert.True(new CriaContaClienteValidator().Validate(Comando(plano: plano)).IsValid);
    }

    [Fact]
    public void Recusa_plano_que_nao_existe()
    {
        var resultado = new CriaContaClienteValidator().Validate(Comando(plano: "PLANO_INVENTADO"));

        Assert.False(resultado.IsValid);
        // Sem esta regra o plano digitado errado era gravado como se existisse, e a conta
        // ficava com um plano que nenhuma regra do sistema reconhece.
        Assert.Contains(resultado.Errors, e => e.ErrorMessage.Contains("Plano inválido"));
    }

    [Theory]
    [InlineData(255, true)]
    [InlineData(256, false)]
    public void Nome_respeita_o_limite_da_coluna(int tamanho, bool deveriaPassar)
    {
        var resultado = new CriaContaClienteValidator().Validate(Comando(nome: new string('x', tamanho)));

        Assert.Equal(deveriaPassar, resultado.IsValid);
    }

    [Fact]
    public void Telefone_e_opcional()
    {
        // Sem telefone o cliente so nao recebe as boas-vindas no WhatsApp -- nao e motivo para
        // impedir o cadastro, ja que o acesso vai por e-mail de qualquer jeito.
        var comando = Comando();
        comando.Telefone = null;

        Assert.True(new CriaContaClienteValidator().Validate(comando).IsValid);
    }
}
