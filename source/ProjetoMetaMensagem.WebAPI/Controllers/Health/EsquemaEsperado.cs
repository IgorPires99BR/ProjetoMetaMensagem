namespace ProjetoMetaMensagem.WebAPI.Controllers.Health
{
    // O que cada migration do BD/ deveria ter deixado no banco. Serve pro endpoint de
    // diagnostico dizer, de fora, se producao esta com o schema em dia -- sem isso a unica
    // forma de saber e alguem com acesso ao banco ir conferir na mao.
    //
    // Cobre da 15 pra frente, que e o intervalo onde producao ja apareceu defasada. Nomes de
    // tabela sao os de DEPOIS da 21 (Flow -> Fluxo, ConversationState -> EstadoConversa etc).
    //
    // Ao criar uma migration nova, acrescente a linha correspondente aqui: o custo e uma linha
    // e evita a proxima rodada de "sera que rodou?".
    public static class EsquemaEsperado
    {
        public record Item(string Migration, string Tabela, string? Coluna = null);

        public static readonly Item[] Itens =
        {
            new("BD/15", "Numero", "TipoConexao"),
            new("BD/15", "Numero", "StatusConexao"),
            new("BD/15", "Numero", "WabaId"),
            new("BD/15", "Numero", "SystemUserToken"),
            new("BD/15", "Numero", "DataUltimaSincronizacao"),

            new("BD/16", "Empresa", "PhoneNumberId"),
            new("BD/16", "Empresa", "WabaId"),

            new("BD/17", "HistoricoDisparo", "StatusEntrega"),

            new("BD/18", "TemplateConexao"),

            new("BD/19", "MensagemRecebida", "MidiaId"),
            new("BD/19", "MensagemRecebida", "TipoMidia"),
            new("BD/19", "HistoricoDisparo", "MidiaId"),
            new("BD/19", "HistoricoDisparo", "TipoMidia"),

            new("BD/20", "Fluxo", "GatilhoInicial"),

            // A 21 renomeou as tabelas: se estes nomes existem, ela rodou.
            new("BD/21", "Fluxo"),
            new("BD/21", "FluxoEtapa"),
            new("BD/21", "EstadoConversa"),
            new("BD/21", "ConfiguracaoWebhook"),

            new("BD/22", "Tag"),
            new("BD/22", "ContatoTag"),
            new("BD/22", "Campanha"),
            new("BD/22", "CampanhaContato"),
            new("BD/22", "Produto"),

            new("BD/23", "EstadoConversa", "AssumidoPorUsuarioId"),
            new("BD/23", "EstadoConversa", "DataAssumido"),

            new("BD/24", "Fluxo", "NumeroId"),

            new("BD/25", "HistoricoDisparo", "MotivoFalha"),

            // O CRM tinha codigo e endpoints, mas nunca teve script: por nao estar nesta lista,
            // o diagnostico respondia "tudo aplicado" enquanto /api/pipeline e /api/leads
            // quebravam com 500 em qualquer ambiente.
            new("BD/26", "Pipeline"),
            new("BD/26", "PipelineEtapa"),
            new("BD/26", "LeadPipeline"),

            new("BD/27", "HistoricoDisparo", "PayloadEnvio"),

            new("BD/28", "Assinatura"),
            new("BD/28", "EventoCakto"),
            new("BD/28", "Empresa", "DataConexaoNumero"),

            new("BD/29", "Assinatura", "UtmSource"),
            new("BD/29", "Assinatura", "Fbc"),

            new("BD/30", "OrigemLead"),

            new("BD/31", "FluxoEtapa", "VariavelSaida"),

            new("BD/32", "FluxoEtapa", "Botao1"),
            new("BD/32", "FluxoEtapa", "Botao2"),
        };
    }
}
