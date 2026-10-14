using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using ProjetoMetaMensagem.Dominio.Entidades;
using ProjetoMetaMensagem.Dominio.Helpers;
using ProjetoMetaMensagem.Dominio.Interfaces.Servicos;
using ProjetoMetaMensagem.Dominio.Servicos;
using ProjetoMetaMensagem.Dominio.UseCases.Contato.CriaContato;
using ProjetoMetaMensagem.Servico.Cobranca;
using ProjetoMetaMensagem.Servico.Configuration;
using Xunit;

namespace ProjetoMetaMensagem.Testes;

// Pix pelo Asaas: o pagador precisa de CPF/CNPJ valido e o split fixo para a subconta da empresa
// nao pode passar do liquido (valor - taxa do Asaas), senao o Asaas bloqueia o repasse DEPOIS
// que o cliente ja pagou.
public class CobrancaAsaasTeste
{
    [Theory]
    [InlineData("529.982.247-25")]
    [InlineData("52998224725")]
    [InlineData("11.222.333/0001-81")]
    public void Documento_com_digito_verificador_certo_e_aceito(string documento)
    {
        Assert.True(DocumentoHelper.CpfCnpjValido(documento));
    }

    [Theory]
    [InlineData("529.982.247-24")]
    [InlineData("111.111.111-11")]
    [InlineData("11.222.333/0001-80")]
    [InlineData("1234")]
    [InlineData("")]
    [InlineData(null)]
    public void Documento_invalido_e_recusado(string? documento)
    {
        Assert.False(DocumentoHelper.CpfCnpjValido(documento));
    }

    [Fact]
    public void Contato_grava_CpfCnpj_so_com_digitos()
    {
        var contato = new Contato(new CriaContatoCommand { Telefone = "11999998888", CpfCnpj = "529.982.247-25" });

        Assert.Equal("52998224725", contato.CpfCnpj);
    }

    [Fact]
    public void Contato_com_CpfCnpj_invalido_nao_passa_na_validacao()
    {
        var resultado = new CriaContatoValidator().Validate(new CriaContatoCommand
        {
            UsuarioId = Guid.NewGuid(),
            EmpresaId = Guid.NewGuid(),
            Telefone = "11999998888",
            CpfCnpj = "123.456.789-00"
        });

        Assert.Contains(resultado.Errors, e => e.PropertyName == nameof(CriaContatoCommand.CpfCnpj));
    }

    [Fact]
    public async Task Comissao_menor_que_a_taxa_do_Asaas_nao_gera_Pix()
    {
        // Sem ApiKey: se o servico chegasse a chamar o Asaas, o erro seria outro.
        var servico = new AsaasPixService(Options.Create(new AsaasConfiguration { TaxaPix = 1.99m }), NullLogger<AsaasPixService>.Instance);
        var cobranca = new NovaCobrancaPix("abc", 100m, DateTime.Today, null,
            new PagadorPix("Fulano", "52998224725", null), "wallet", ValorRepasse: 99m);

        var erro = await Assert.ThrowsAsync<PixException>(() => servico.CriarCobrancaAsync(cobranca));

        Assert.Contains("taxa do Asaas", erro.Message);
    }

    [Fact]
    public void Comissao_percentual_sai_da_parte_da_empresa()
    {
        var r = CalculoComissao.Calcular(405m, DadosBancariosEmpresa.ComissaoPercentual, 3m)!;

        Assert.Equal(12.15m, r.Comissao);
        Assert.Equal(392.85m, r.Repasse);
    }

    [Fact]
    public void Pix_so_conta_como_pago_quando_o_Asaas_diz_RECEIVED()
    {
        // CONFIRMED no Pix e o bloqueio cautelar de ate 72h: dinheiro ainda nao disponivel.
        Assert.True(new ConsultaCobrancaPix("RECEIVED", 10m, DateTime.Now, null).Paga);
        Assert.False(new ConsultaCobrancaPix("CONFIRMED", 10m, DateTime.Now, null).Paga);
        Assert.False(new ConsultaCobrancaPix("PENDING", null, null, null).Paga);
    }
}
