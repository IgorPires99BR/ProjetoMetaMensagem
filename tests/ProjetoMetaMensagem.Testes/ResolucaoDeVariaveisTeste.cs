using ProjetoMetaMensagem.Dominio.Entidades;
using ProjetoMetaMensagem.Dominio.Servicos;
using ProjetoMetaMensagem.Dominio.UseCases.Messages.EnviarMensagemTemplateMetaLote;
using Xunit;

namespace ProjetoMetaMensagem.Testes;

// Disparos e Agendamentos resolviam as variaveis do template cada um do seu jeito, e um campo
// novo (valorFatura) so existia num dos lados. Agora os dois passam por ResolvedorDeVariaveis;
// estes testes travam as regras que o cliente enxerga na mensagem: formato pt-BR do valor e a
// data de vencimento no mes de referencia (dia 20 em setembro -> 20/09/2026).
public class ResolucaoDeVariaveisTeste
{
    private static readonly DateTime Setembro2026 = new(2026, 9, 5);

    private static Contato ContatoDeExemplo(int? diaVencimento = 20, decimal? valorFatura = 405m) => new()
    {
        Telefone = "5511999990000",
        NomeContato = "Maria",
        NomeCliente = "Maria Souza",
        DiaVencimento = diaVencimento,
        ValorFatura = valorFatura,
        TaxaJuros = 2m,
    };

    private static readonly IReadOnlyDictionary<Guid, Parametro> SemParametros = new Dictionary<Guid, Parametro>();

    [Fact]
    public void Data_de_vencimento_aplica_o_dia_do_contato_ao_mes_atual()
    {
        var texto = ResolvedorDeVariaveis.DataDeVencimento(20, Setembro2026);

        Assert.Equal("20/09/2026", texto);
    }

    [Theory]
    [InlineData(31, 2026, 2, "28/02/2026")] // fevereiro nao tem dia 31
    [InlineData(31, 2026, 4, "30/04/2026")]
    [InlineData(29, 2028, 2, "29/02/2028")] // ano bissexto mantem o 29
    public void Data_de_vencimento_e_clampada_pro_ultimo_dia_do_mes(int dia, int ano, int mes, string esperado)
    {
        Assert.Equal(esperado, ResolvedorDeVariaveis.DataDeVencimento(dia, new DateTime(ano, mes, 10)));
    }

    [Fact]
    public void Contato_sem_dia_de_vencimento_fica_com_data_vazia()
    {
        Assert.Equal(string.Empty, ResolvedorDeVariaveis.DataDeVencimento(null, Setembro2026));
    }

    [Fact]
    public void Valor_da_fatura_sai_em_formato_brasileiro_com_duas_casas()
    {
        var variavel = new AgendamentoVariavelDto { Origem = ResolvedorDeVariaveis.ValorFatura };

        var texto = ResolvedorDeVariaveis.Resolver(variavel, ContatoDeExemplo(valorFatura: 1234.5m), SemParametros, Setembro2026);

        Assert.Equal("1.234,50", texto);
    }

    [Fact]
    public void Parametro_fixo_devolve_o_texto_cadastrado_igual_pra_todos()
    {
        var parametro = new Parametro { Nome = "Chave PIX", Tipo = Parametro.Fixo, Valor = "12.345.678/0001-90" };
        var parametros = new Dictionary<Guid, Parametro> { [parametro.Id] = parametro };
        var variavel = new AgendamentoVariavelDto { Origem = ResolvedorDeVariaveis.ParametroCadastrado, ParametroId = parametro.Id };

        Assert.False(ResolvedorDeVariaveis.DependeDoContato(variavel, parametros));
        Assert.Equal("12.345.678/0001-90", ResolvedorDeVariaveis.Resolver(variavel, null, parametros, Setembro2026));
    }

    [Fact]
    public void Parametro_de_campo_do_contato_resolve_por_destinatario()
    {
        var parametro = new Parametro { Nome = "Vencimento", Tipo = Parametro.CampoDoContato, Valor = ResolvedorDeVariaveis.DataVencimento };
        var parametros = new Dictionary<Guid, Parametro> { [parametro.Id] = parametro };
        var variavel = new AgendamentoVariavelDto { Origem = ResolvedorDeVariaveis.ParametroCadastrado, ParametroId = parametro.Id };

        Assert.True(ResolvedorDeVariaveis.DependeDoContato(variavel, parametros));
        Assert.Equal("20/09/2026", ResolvedorDeVariaveis.Resolver(variavel, ContatoDeExemplo(20), parametros, Setembro2026));
        Assert.Equal("15/09/2026", ResolvedorDeVariaveis.Resolver(variavel, ContatoDeExemplo(15), parametros, Setembro2026));
    }

    [Fact]
    public void Parametro_inexistente_resolve_vazio_em_vez_de_estourar()
    {
        var variavel = new AgendamentoVariavelDto { Origem = ResolvedorDeVariaveis.ParametroCadastrado, ParametroId = Guid.NewGuid() };

        Assert.Equal(string.Empty, ResolvedorDeVariaveis.Resolver(variavel, ContatoDeExemplo(), SemParametros, Setembro2026));
    }

    [Fact]
    public void Preencher_separa_valor_global_do_valor_por_telefone()
    {
        var pix = new Parametro { Nome = "Chave PIX", Tipo = Parametro.Fixo, Valor = "pix@empresa.com" };
        var parametros = new Dictionary<Guid, Parametro> { [pix.Id] = pix };
        var variaveis = new List<AgendamentoVariavelDto>
        {
            new() { Origem = ResolvedorDeVariaveis.NomeCliente },
            new() { Origem = ResolvedorDeVariaveis.DataVencimento },
            new() { Origem = ResolvedorDeVariaveis.ValorFatura },
            new() { Origem = ResolvedorDeVariaveis.ParametroCadastrado, ParametroId = pix.Id },
            new() { Origem = ResolvedorDeVariaveis.Fixo, ValorFixo = "  obrigado  " },
        };
        var comando = new EnviarMensagemTemplateMetaLoteCommand();
        var ana = ContatoDeExemplo(20, 405m);
        var bruno = ContatoDeExemplo(31, 99.9m);

        ResolvedorDeVariaveis.Preencher(comando, variaveis,
            new[] { ("5511111111111", ana), ("5522222222222", bruno) },
            parametros, Setembro2026);

        // Slot "global" fica vazio nas variaveis que dependem do contato; so os textos fixos entram.
        Assert.Equal(new[] { "", "", "", "pix@empresa.com", "obrigado" }, comando.ParametrosBody);

        Assert.Equal(new[] { "Maria Souza", "20/09/2026", "405,00", "pix@empresa.com", "obrigado" },
            comando.ParametrosBodyDe("5511111111111", ana.Id.ToString()));
        // Dia 31 em setembro (30 dias) cai no ultimo dia do mes.
        Assert.Equal(new[] { "Maria Souza", "30/09/2026", "99,90", "pix@empresa.com", "obrigado" },
            comando.ParametrosBodyDe("5522222222222", bruno.Id.ToString()));
    }

    [Fact]
    public void Preencher_so_com_textos_fixos_nao_cria_valores_por_telefone()
    {
        var variaveis = new List<AgendamentoVariavelDto> { new() { Origem = ResolvedorDeVariaveis.Fixo, ValorFixo = "Promo" } };
        var comando = new EnviarMensagemTemplateMetaLoteCommand();

        ResolvedorDeVariaveis.Preencher(comando, variaveis, new[] { ("5511111111111", ContatoDeExemplo()) }, SemParametros, Setembro2026);

        Assert.Equal(new[] { "Promo" }, comando.ParametrosBody);
        Assert.Empty(comando.ParametrosBodyPorTelefone);
        Assert.Empty(comando.ParametrosBodyPorContato);
    }

    // Uma pessoa cuida de varias empresas e o mesmo WhatsApp esta cadastrado em mais de um
    // contato (na Sebrecon, 8 numeros / 18 contatos). Com valores indexados por telefone, o
    // ultimo contato sobrescrevia os outros: todos recebiam a fatura e o vencimento do ultimo.
    [Fact]
    public void Contatos_com_o_mesmo_telefone_recebem_cada_um_os_proprios_valores()
    {
        var variaveis = new List<AgendamentoVariavelDto>
        {
            new() { Origem = ResolvedorDeVariaveis.NomeCliente },
            new() { Origem = ResolvedorDeVariaveis.ValorFatura },
            new() { Origem = ResolvedorDeVariaveis.DataVencimento },
        };
        var americaXis = ContatoDeExemplo(20, 505m); americaXis.NomeCliente = "AMERICA - XIS ALUGUEL";
        var boutique = ContatoDeExemplo(30, 355m); boutique.NomeCliente = "BOUTIQUE - SIMONE";
        const string telefoneDoAdemir = "5511978737005";

        var comando = new EnviarMensagemTemplateMetaLoteCommand
        {
            Telefones = new List<string> { telefoneDoAdemir, telefoneDoAdemir },
            ContatosIds = new List<string> { americaXis.Id.ToString(), boutique.Id.ToString() },
        };

        ResolvedorDeVariaveis.Preencher(comando, variaveis,
            new[] { (telefoneDoAdemir, americaXis), (telefoneDoAdemir, boutique) }, SemParametros, Setembro2026);

        Assert.Equal(new[] { "AMERICA - XIS ALUGUEL", "505,00", "20/09/2026" }, comando.ParametrosBodyDoDestinatario(0));
        Assert.Equal(new[] { "BOUTIQUE - SIMONE", "355,00", "30/09/2026" }, comando.ParametrosBodyDoDestinatario(1));
        Assert.True(comando.TemValoresPorDestinatario);
    }

    [Fact]
    public void Validador_do_lote_aceita_telefone_repetido_com_valores_por_contato()
    {
        var variaveis = new List<AgendamentoVariavelDto> { new() { Origem = ResolvedorDeVariaveis.ValorFatura } };
        var a = ContatoDeExemplo(20, 505m);
        var b = ContatoDeExemplo(30, 355m);
        var comando = new EnviarMensagemTemplateMetaLoteCommand
        {
            IdEmpresa = Guid.NewGuid(),
            NomeTemplate = "cobranca",
            Telefones = new List<string> { "5511978737005", "5511978737005" },
            ContatosIds = new List<string> { a.Id.ToString(), b.Id.ToString() },
        };
        ResolvedorDeVariaveis.Preencher(comando, variaveis,
            new[] { ("5511978737005", a), ("5511978737005", b) }, SemParametros, Setembro2026);

        var resultado = new EnviarMensagemTemplateMetaLoteValidator().Validate(comando);

        Assert.True(resultado.IsValid, string.Join("; ", resultado.Errors.Select(e => e.ErrorMessage)));
    }

    [Fact]
    public void Validador_do_lote_barra_contato_sem_valor_mesmo_com_telefone_repetido()
    {
        var variaveis = new List<AgendamentoVariavelDto> { new() { Origem = ResolvedorDeVariaveis.ValorFatura } };
        var completo = ContatoDeExemplo(20, 505m);
        var semFatura = ContatoDeExemplo(30, null);
        var comando = new EnviarMensagemTemplateMetaLoteCommand
        {
            IdEmpresa = Guid.NewGuid(),
            NomeTemplate = "cobranca",
            Telefones = new List<string> { "5511978737005", "5511978737005" },
            ContatosIds = new List<string> { completo.Id.ToString(), semFatura.Id.ToString() },
        };
        ResolvedorDeVariaveis.Preencher(comando, variaveis,
            new[] { ("5511978737005", completo), ("5511978737005", semFatura) }, SemParametros, Setembro2026);

        var resultado = new EnviarMensagemTemplateMetaLoteValidator().Validate(comando);

        Assert.False(resultado.IsValid);
    }

    [Theory]
    [InlineData("fixo", true)]
    [InlineData("parametro", true)]
    [InlineData("valorFatura", true)]
    [InlineData("dataVencimento", true)]
    [InlineData("campoQueNaoExiste", false)]
    [InlineData(null, false)]
    public void So_origens_conhecidas_sao_validas(string? origem, bool esperado)
    {
        Assert.Equal(esperado, ResolvedorDeVariaveis.OrigemValida(origem));
    }
}
