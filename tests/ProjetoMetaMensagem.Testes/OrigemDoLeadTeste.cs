using ProjetoMetaMensagem.Dominio.Entidades;
using ProjetoMetaMensagem.Dominio.UseCases.Contato.ObtemContato;
using Xunit;

namespace ProjetoMetaMensagem.Testes;

// OrigemLead e gravada na primeira mensagem de quem chega por um anuncio Click-to-WhatsApp,
// mas ate agora nunca era lida fora do momento da compra -- a tela de Contatos nao mostrava
// de qual anuncio um lead vinha, e com zero vendas ate agora esse dado nunca tinha sido visto
// por ninguem, nem pra confirmar se a captura estava funcionando.
//
// O que este teste protege: quando o mesmo telefone tem mais de um registro de origem (a
// pessoa reapareceu por um segundo anuncio depois que a conversa reiniciou), a origem mostrada
// tem que continuar sendo a PRIMEIRA -- foi ela que teve o merito de trazer o lead.
public class OrigemDoLeadTeste
{
    private static OrigemLead Origem(string telefone, DateTime dataPrimeiroContato, string headline) =>
        new()
        {
            Id = Guid.NewGuid(),
            EmpresaId = Guid.NewGuid(),
            Telefone = telefone,
            DataPrimeiroContato = dataPrimeiroContato,
            Headline = headline,
        };

    [Fact]
    public void Telefone_com_uma_origem_so_aparece_direto()
    {
        var origem = Origem("5511900000000", DateTime.Now, "Cansado de perder cliente por demorar a responder?");

        var mapa = ObtemContatoHandler.AgruparOrigemMaisAntigaPorTelefone(new[] { origem });

        Assert.Same(origem, mapa["5511900000000"]);
    }

    [Fact]
    public void Telefone_com_duas_origens_fica_com_a_mais_antiga()
    {
        var primeira = Origem("5511900000000", new DateTime(2026, 8, 1), "Anuncio A - primeiro contato");
        var segunda = Origem("5511900000000", new DateTime(2026, 8, 20), "Anuncio B - reapareceu depois");

        // Passadas fora de ordem de proposito -- a lista do banco nao vem ordenada do jeito
        // que o teste precisa.
        var mapa = ObtemContatoHandler.AgruparOrigemMaisAntigaPorTelefone(new[] { segunda, primeira });

        Assert.Same(primeira, mapa["5511900000000"]);
    }

    [Fact]
    public void Telefones_diferentes_nao_se_misturam()
    {
        var deA = Origem("5511900000001", DateTime.Now, "Anuncio A");
        var deB = Origem("5511900000002", DateTime.Now, "Anuncio B");

        var mapa = ObtemContatoHandler.AgruparOrigemMaisAntigaPorTelefone(new[] { deA, deB });

        Assert.Equal(2, mapa.Count);
        Assert.Same(deA, mapa["5511900000001"]);
        Assert.Same(deB, mapa["5511900000002"]);
    }

    [Fact]
    public void Sem_origem_nenhuma_o_mapa_fica_vazio_e_o_contato_nao_quebra()
    {
        var mapa = ObtemContatoHandler.AgruparOrigemMaisAntigaPorTelefone(Array.Empty<OrigemLead>());

        Assert.Empty(mapa);

        var resultado = new ObtemContatoResult(new Contato
        {
            Id = Guid.NewGuid(),
            UsuarioId = Guid.NewGuid(),
            Telefone = "5511900000000",
            DataCriacao = DateTime.Now,
        });

        // Contato cadastrado manualmente ou que escreveu organicamente, sem passar por
        // anuncio nenhum -- OrigemAnuncio precisa ficar null, nao vazio nem quebrar.
        Assert.Null(resultado.OrigemAnuncio);
        Assert.Null(resultado.OrigemData);
    }

    [Fact]
    public void Sem_headline_cai_no_SourceId()
    {
        var origem = Origem("5511900000000", DateTime.Now, headline: null!);
        origem.SourceId = "120210000000000";

        var resultado = new ObtemContatoResult(new Contato
        {
            Id = Guid.NewGuid(),
            UsuarioId = Guid.NewGuid(),
            Telefone = "5511900000000",
            DataCriacao = DateTime.Now,
        }, origem);

        // A Meta nem sempre manda o headline do anuncio no referral -- sem essa rede, um lead
        // legitimamente vindo de anuncio apareceria como se nao tivesse origem nenhuma.
        Assert.Equal("120210000000000", resultado.OrigemAnuncio);
    }
}
