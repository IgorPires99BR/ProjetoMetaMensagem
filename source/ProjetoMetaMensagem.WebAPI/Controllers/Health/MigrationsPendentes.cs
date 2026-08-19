namespace ProjetoMetaMensagem.WebAPI.Controllers.Health
{
    // DDL das migrations que o endpoint /api/health/aplicar-schema sabe executar.
    //
    // O SQL fica AQUI, versionado no codigo -- o endpoint nunca executa comando vindo da
    // requisicao. Quem chama escolhe no maximo QUAIS destas rodar, jamais o que elas fazem.
    //
    // Cada item so roda se a checagem (Tabela/Coluna) apontar que ainda falta, entao chamar
    // duas vezes nao quebra nada. Ao criar uma migration nova, acrescente aqui e em
    // EsquemaEsperado -- as duas listas juntas fazem "conferir" e "consertar" andarem par a par.
    public static class MigrationsPendentes
    {
        // Sql roda so quando a Coluna (ou a Tabela, se Coluna for nula) nao existe ainda.
        public record Item(string Migration, string Tabela, string? Coluna, string Sql);

        public static readonly Item[] Itens =
        {
            new("BD/31", "FluxoEtapa", "VariavelSaida",
                "ALTER TABLE FluxoEtapa ADD VariavelSaida NVARCHAR(100) NULL;"),

            new("BD/32", "FluxoEtapa", "Botao1",
                "ALTER TABLE FluxoEtapa ADD Botao1 NVARCHAR(20) NULL;"),

            new("BD/32", "FluxoEtapa", "Botao2",
                "ALTER TABLE FluxoEtapa ADD Botao2 NVARCHAR(20) NULL;"),

            new("BD/33", "MensagemRecebida", "WamidRecebido",
                "ALTER TABLE MensagemRecebida ADD WamidRecebido NVARCHAR(100) NULL;"),

            new("BD/35", "FluxoEtapa", "ProximaEtapaIdB",
                "ALTER TABLE FluxoEtapa ADD ProximaEtapaIdB UNIQUEIDENTIFIER NULL;"),

            new("BD/36", "EstadoConversa", "ProcessandoAte",
                "ALTER TABLE EstadoConversa ADD ProcessandoAte DATETIME NULL;"),
        };
    }
}
