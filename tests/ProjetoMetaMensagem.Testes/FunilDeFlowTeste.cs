using ProjetoMetaMensagem.Dominio.Entidades;
using ProjetoMetaMensagem.Dominio.UseCases.Relatorio.ObtemFunilFlow;
using Xunit;

namespace ProjetoMetaMensagem.Testes;

// Ate agora nao existia nenhuma forma de ver EM QUAL ETAPA do flow as conversas param -- so
// dava para saber quantas pessoas responderam no total (relatorio de engajamento). Com a
// campanha gastando e 18 de 24 leads respondendo mas zero comprando, essa pergunta e o que
// decide se o problema e o flow ser longo demais, a etapa do plano confundir, ou o publico
// simplesmente nao ser o certo -- e sem dado nenhum, qualquer resposta seria chute.
//
// O que estes testes protegem: a ordenacao das etapas (o banco nao guarda posicao, so o
// encadeamento) e a contagem dos tres desfechos possiveis de uma conversa -- presa, entregue
// a atendente, ou concluida -- que precisam ser mutuamente exclusivos.
public class FunilDeFlowTeste
{
    private static FlowEtapa Etapa(Guid id, bool inicial = false, Guid? proxima = null, Guid? proximaB = null, string nome = "Mensagem") =>
        new()
        {
            Id = id,
            FlowId = Guid.NewGuid(),
            NomeEtapa = nome,
            ConteudoLivre = $"conteudo da etapa {id.ToString()[..4]}",
            GatilhoResposta = "Avancar",
            EhEtapaInicial = inicial,
            ProximaEtapaId = proxima,
            ProximaEtapaIdB = proximaB,
        };

    private static ConversationState Conversa(Guid etapaId, bool finalizado = false, bool aguardandoAtendente = false) =>
        new()
        {
            Id = Guid.NewGuid(),
            FlowId = Guid.NewGuid(),
            ContatoId = Guid.NewGuid(),
            EtapaAtualId = etapaId,
            DataInicio = DateTime.Now,
            Finalizado = finalizado,
            AguardandoAtendente = aguardandoAtendente,
        };

    [Fact]
    public void Ordena_as_etapas_seguindo_o_encadeamento_a_partir_da_inicial()
    {
        var e3 = Etapa(Guid.NewGuid());
        var e2 = Etapa(Guid.NewGuid(), proxima: e3.Id);
        var e1 = Etapa(Guid.NewGuid(), inicial: true, proxima: e2.Id);
        e2.ProximaEtapaId = e3.Id; e1.ProximaEtapaId = e2.Id;

        // Passadas fora de ordem de proposito -- a lista do banco nao vem ordenada.
        var etapas = new List<FlowEtapa> { e3, e1, e2 };

        var resultado = ObtemFunilFlowHandler.MontarFunil(Guid.NewGuid(), "Teste", etapas, new List<ConversationState>());

        Assert.Equal(new[] { e1.Id, e2.Id, e3.Id }, resultado.Etapas.Select(x => x.EtapaId));
        Assert.Equal(new[] { 1, 2, 3 }, resultado.Etapas.Select(x => x.Ordem));
    }

    [Fact]
    public void Ramo_B_do_botao_entra_na_ordem_mesmo_sem_ser_o_caminho_principal()
    {
        // "Quanto custa?" (Botao2) pula direto pra etapa de planos, sem passar pela explicacao.
        var planos = Etapa(Guid.NewGuid(), nome: "Planos");
        var explicacao = Etapa(Guid.NewGuid(), proxima: planos.Id, nome: "Explicacao");
        var pergunta = Etapa(Guid.NewGuid(), inicial: true, proxima: explicacao.Id, proximaB: planos.Id, nome: "Pergunta");

        var etapas = new List<FlowEtapa> { pergunta, explicacao, planos };

        var resultado = ObtemFunilFlowHandler.MontarFunil(Guid.NewGuid(), "Teste", etapas, new List<ConversationState>());

        // Planos e alcancavel por dois caminhos (via explicacao OU direto pelo ramo B) --
        // precisa aparecer uma unica vez, nao duas.
        Assert.Equal(3, resultado.Etapas.Count);
        Assert.Contains(resultado.Etapas, x => x.EtapaId == planos.Id);
    }

    [Fact]
    public void Etapa_sem_proximo_e_marcada_como_final()
    {
        var fim = Etapa(Guid.NewGuid(), nome: "Ultima");
        var inicio = Etapa(Guid.NewGuid(), inicial: true, proxima: fim.Id);
        var etapas = new List<FlowEtapa> { inicio, fim };

        var resultado = ObtemFunilFlowHandler.MontarFunil(Guid.NewGuid(), "Teste", etapas, new List<ConversationState>());

        Assert.False(resultado.Etapas.First(x => x.EtapaId == inicio.Id).EhEtapaFinal);
        Assert.True(resultado.Etapas.First(x => x.EtapaId == fim.Id).EhEtapaFinal);
    }

    [Fact]
    public void Os_tres_desfechos_sao_contados_separados_e_batem_com_o_total()
    {
        var etapaPlano = Etapa(Guid.NewGuid(), nome: "Escolha o plano");
        var etapaInicial = Etapa(Guid.NewGuid(), inicial: true, proxima: etapaPlano.Id);
        var etapas = new List<FlowEtapa> { etapaInicial, etapaPlano };

        var conversas = new List<ConversationState>
        {
            Conversa(etapaInicial.Id),                                       // presa, nem chegou no plano
            Conversa(etapaPlano.Id),                                          // presa, esta na etapa do plano
            Conversa(etapaPlano.Id, aguardandoAtendente: true),               // desconversou e foi pro atendente
            Conversa(etapaPlano.Id, finalizado: true),                        // concluiu o flow
            Conversa(etapaPlano.Id, finalizado: true),
        };

        var resultado = ObtemFunilFlowHandler.MontarFunil(Guid.NewGuid(), "Teste", etapas, conversas);

        Assert.Equal(5, resultado.TotalConversas);
        Assert.Equal(2, resultado.TotalPresas);
        Assert.Equal(1, resultado.TotalEntreguesAoAtendente);
        Assert.Equal(2, resultado.TotalConcluiram);

        var linhaPlano = resultado.Etapas.First(x => x.EtapaId == etapaPlano.Id);
        Assert.Equal(1, linhaPlano.Presas);
        Assert.Equal(1, linhaPlano.EntreguesAoAtendente);
        Assert.Equal(2, linhaPlano.Concluiram);

        // Uma conversa finalizada e AguardandoAtendente ao mesmo tempo nao pode ser contada
        // duas vezes -- Finalizado manda: se acabou, acabou.
        Assert.Equal(resultado.TotalConversas, resultado.TotalPresas + resultado.TotalEntreguesAoAtendente + resultado.TotalConcluiram);
    }

    [Fact]
    public void Conversa_finalizada_e_aguardando_atendente_ao_mesmo_tempo_conta_como_concluida()
    {
        var etapa = Etapa(Guid.NewGuid(), inicial: true);
        var conversas = new List<ConversationState> { Conversa(etapa.Id, finalizado: true, aguardandoAtendente: true) };

        var resultado = ObtemFunilFlowHandler.MontarFunil(Guid.NewGuid(), "Teste", new List<FlowEtapa> { etapa }, conversas);

        Assert.Equal(1, resultado.TotalConcluiram);
        Assert.Equal(0, resultado.TotalEntreguesAoAtendente);
    }

    [Fact]
    public void Conversa_numa_etapa_que_nao_existe_mais_ainda_conta_no_total()
    {
        // Uma edicao de flow pode excluir uma etapa depois que conversas ja passaram por ela
        // (AlteraFlowHandler bloqueia so quando a conversa esta EXATAMENTE naquela etapa no
        // momento da edicao -- uma que ja passou e seguiu em frente fica com um EtapaAtualId
        // que nao existe mais). O total da tela nao pode fingir que essa pessoa nao existiu.
        var etapaViva = Etapa(Guid.NewGuid(), inicial: true);
        var conversas = new List<ConversationState> { Conversa(Guid.NewGuid()) };

        var resultado = ObtemFunilFlowHandler.MontarFunil(Guid.NewGuid(), "Teste", new List<FlowEtapa> { etapaViva }, conversas);

        Assert.Equal(1, resultado.TotalConversas);
        Assert.Equal(1, resultado.TotalPresas);
        Assert.Equal(0, resultado.Etapas.Single().Presas);
    }
}
