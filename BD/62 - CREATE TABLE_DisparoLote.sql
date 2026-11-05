USE ContactSolutionDB;
GO

-- Fila do disparo em massa. O disparo rodava inteiro dentro de um request HTTP: um timeout no meio
-- deixava parte das mensagens enviadas e sem registro. Agora o request so enfileira e o
-- DisparoLoteWorker processa um contato por vez, com a situacao de cada um gravada aqui.
IF OBJECT_ID(N'[dbo].[DisparoLote]', N'U') IS NULL
BEGIN
    CREATE TABLE DisparoLote (
        Id UNIQUEIDENTIFIER PRIMARY KEY,
        EmpresaId UNIQUEIDENTIFIER NOT NULL,
        TemplateId UNIQUEIDENTIFIER NOT NULL,
        -- Gerada pela tela: repetir o POST depois de um timeout devolve o mesmo lote em vez de
        -- cobrar todo mundo de novo.
        ChaveIdempotencia UNIQUEIDENTIFIER NOT NULL,
        VariaveisJson NVARCHAR(MAX) NULL,
        ParametroHeaderMediaUrl NVARCHAR(1000) NULL,
        ParametrosButtonJson NVARCHAR(MAX) NULL,
        DataCriacao DATETIME NOT NULL,
        CONSTRAINT FK_DisparoLote_Empresa FOREIGN KEY (EmpresaId) REFERENCES Empresa(Id),
        CONSTRAINT FK_DisparoLote_Template FOREIGN KEY (TemplateId) REFERENCES Template(Id),
        CONSTRAINT UQ_DisparoLote_Chave UNIQUE (EmpresaId, ChaveIdempotencia)
    );
    CREATE INDEX IX_DisparoLote_Empresa_Data ON DisparoLote (EmpresaId, DataCriacao DESC);
END
GO

IF OBJECT_ID(N'[dbo].[DisparoLoteItem]', N'U') IS NULL
BEGIN
    CREATE TABLE DisparoLoteItem (
        Id UNIQUEIDENTIFIER PRIMARY KEY,
        LoteId UNIQUEIDENTIFIER NOT NULL,
        ContatoId UNIQUEIDENTIFIER NOT NULL,
        Ordem INT NOT NULL,
        -- PENDENTE, PREPARANDO, ENVIANDO, ENVIADO, FALHOU ou INCERTO (ver StatusDisparoItem).
        Status NVARCHAR(20) NOT NULL,
        Tentativas INT NOT NULL CONSTRAINT DF_DisparoLoteItem_Tentativas DEFAULT (0),
        -- Em UTC, sempre comparadas com GETUTCDATE(): nao dependem do relogio de quem processa.
        ProximaTentativaEm DATETIME NOT NULL,
        ReservadoAte DATETIME NULL,
        Erro NVARCHAR(1000) NULL,
        -- Mensagem e cobranca (com o Pix) prontas para envio, gravadas ANTES de falar com a Meta:
        -- nova tentativa reaproveita o mesmo Pix, e uma queda no meio ainda consegue registrar a
        -- cobranca. Apagadas quando o item termina.
        EnvioJson NVARCHAR(MAX) NULL,
        CobrancaJson NVARCHAR(MAX) NULL,
        DataAtualizacao DATETIME NULL,
        CONSTRAINT FK_DisparoLoteItem_Lote FOREIGN KEY (LoteId) REFERENCES DisparoLote(Id) ON DELETE CASCADE,
        CONSTRAINT FK_DisparoLoteItem_Contato FOREIGN KEY (ContatoId) REFERENCES Contato(Id),
        CONSTRAINT UQ_DisparoLoteItem_Lote_Contato UNIQUE (LoteId, ContatoId)
    );
    CREATE INDEX IX_DisparoLoteItem_Fila ON DisparoLoteItem (Status, ProximaTentativaEm, Ordem);
    CREATE INDEX IX_DisparoLoteItem_Contato ON DisparoLoteItem (ContatoId, Status);
END
GO
