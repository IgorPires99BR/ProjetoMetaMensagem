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
            // Sem esta coluna a consulta unificada da conversa (MensagemRecebida + HistoricoDisparo)
            // estoura "Invalid column name 'MotivoFalha'" e o chat nao carrega nenhuma mensagem.
            new("BD/25", "HistoricoDisparo", "MotivoFalha",
                "ALTER TABLE HistoricoDisparo ADD MotivoFalha NVARCHAR(500) NULL;"),

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

            new("BD/37", "Numero", "TokenExpiraEm",
                "ALTER TABLE Numero ADD TokenExpiraEm DATETIME NULL;"),

            new("BD/38", "EstadoConversa", "TentativasNaEtapa",
                "ALTER TABLE EstadoConversa ADD TentativasNaEtapa INT NOT NULL DEFAULT 0;"),

            new("BD/38", "EstadoConversa", "AguardandoAtendente",
                "ALTER TABLE EstadoConversa ADD AguardandoAtendente BIT NOT NULL DEFAULT 0;"),

            new("BD/39", "Fluxo", "SourceIdAnuncio",
                "ALTER TABLE Fluxo ADD SourceIdAnuncio NVARCHAR(60) NULL;"),

            // DataReferencia e VariaveisJson entraram direto no CREATE TABLE da migration 40
            // numa revisao posterior, sem ALTER incremental -- ver EsquemaEsperado.cs.
            // DataReferencia e NOT NULL: cria com default temporario, faz o backfill a partir
            // de ProximaExecucao (mesma data/hora que ja funcionava como referencia antes desta
            // coluna existir) e remove o default -- coluna nova some sem exigir um valor fixo
            // artificial nem quebrar linha ja existente.
            new("BD/40", "Agendamento", "DataReferencia",
                @"ALTER TABLE Agendamento ADD DataReferencia DATETIME NOT NULL
                    CONSTRAINT DF_Agendamento_DataReferencia DEFAULT ('19000101');
                  UPDATE Agendamento SET DataReferencia = ProximaExecucao WHERE DataReferencia = '19000101';
                  ALTER TABLE Agendamento DROP CONSTRAINT DF_Agendamento_DataReferencia;"),

            new("BD/40", "Agendamento", "VariaveisJson",
                "ALTER TABLE Agendamento ADD VariaveisJson NVARCHAR(MAX) NULL;"),

            new("BD/41", "Agendamento", "DiasSemana",
                "ALTER TABLE Agendamento ADD DiasSemana NVARCHAR(20) NULL;"),

            new("BD/41", "Agendamento", "DiaDoMes",
                "ALTER TABLE Agendamento ADD DiaDoMes INT NULL;"),

            new("BD/46", "Parametro", null,
                @"CREATE TABLE Parametro (
                    Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
                    EmpresaId UNIQUEIDENTIFIER NOT NULL,
                    Nome NVARCHAR(100) NOT NULL,
                    Descricao NVARCHAR(255) NULL,
                    Tipo NVARCHAR(20) NOT NULL,
                    Valor NVARCHAR(500) NOT NULL,
                    DataCriacao DATETIME DEFAULT GETDATE(),
                    CONSTRAINT FK_Parametro_Empresa FOREIGN KEY (EmpresaId) REFERENCES Empresa(Id),
                    CONSTRAINT UQ_Parametro_Empresa_Nome UNIQUE (EmpresaId, Nome)
                  );"),

            new("BD/47", "Template", "GeraCobranca",
                "ALTER TABLE Template ADD GeraCobranca BIT NOT NULL DEFAULT (0);"),

            // CobrancaCliente depende de Contato/Template/HistoricoDisparo ja existirem -- todos
            // anteriores a esta migration, entao a ordem do array e suficiente.
            new("BD/48", "CobrancaCliente", null,
                @"CREATE TABLE CobrancaCliente (
                    Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
                    EmpresaId UNIQUEIDENTIFIER NOT NULL,
                    ContatoId UNIQUEIDENTIFIER NOT NULL,
                    TemplateId UNIQUEIDENTIFIER NOT NULL,
                    HistoricoDisparoId UNIQUEIDENTIFIER NOT NULL,
                    Valor DECIMAL(10,2) NOT NULL,
                    DataVencimento DATETIME NOT NULL,
                    Status NVARCHAR(20) NOT NULL DEFAULT ('PENDENTE'),
                    DataPagamento DATETIME NULL,
                    UtmContentCakto NVARCHAR(100) NULL,
                    EventoIdCakto NVARCHAR(100) NULL,
                    DataCriacao DATETIME NOT NULL DEFAULT (GETDATE()),
                    DataAtualizacao DATETIME NULL,
                    CONSTRAINT FK_CobrancaCliente_Empresa FOREIGN KEY (EmpresaId) REFERENCES Empresa(Id),
                    CONSTRAINT FK_CobrancaCliente_Contato FOREIGN KEY (ContatoId) REFERENCES Contato(Id),
                    CONSTRAINT FK_CobrancaCliente_Template FOREIGN KEY (TemplateId) REFERENCES Template(Id),
                    CONSTRAINT FK_CobrancaCliente_HistoricoDisparo FOREIGN KEY (HistoricoDisparoId) REFERENCES HistoricoDisparo(Id)
                  );
                  CREATE INDEX IX_CobrancaCliente_Empresa_Status ON CobrancaCliente (EmpresaId, Status, DataVencimento);"),
        };
    }
}
