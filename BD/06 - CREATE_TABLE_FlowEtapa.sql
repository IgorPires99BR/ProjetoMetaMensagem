USE ContactSolutionDB

IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[FlowEtapa]') AND type in (N'U'))
BEGIN
    CREATE TABLE dbo.FlowEtapa (
        Id UNIQUEIDENTIFIER NOT NULL DEFAULT NEWID(),
        FlowId UNIQUEIDENTIFIER NOT NULL,
        TemplateId UNIQUEIDENTIFIER NULL,
        NomeEtapa NVARCHAR(100) NOT NULL,
        ConteudoLivre NVARCHAR(MAX) NULL, 
        GatilhoResposta NVARCHAR(100) NULL, 
        ProximaEtapaId UNIQUEIDENTIFIER NULL, 
        EhEtapaInicial BIT NOT NULL DEFAULT 0, 

        CONSTRAINT PK_FlowEtapa PRIMARY KEY CLUSTERED (Id)
    );
END
GO

-- FK com a tabela agrupadora Flow
IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_FlowEtapa_Flow]') AND parent_object_id = OBJECT_ID(N'[dbo].[FlowEtapa]'))
BEGIN
    ALTER TABLE dbo.FlowEtapa
    ADD CONSTRAINT FK_FlowEtapa_Flow FOREIGN KEY (FlowId) 
    REFERENCES dbo.Flow (Id) ON DELETE CASCADE;
END
GO

-- FK com a tabela de Template do seu CRM
IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_FlowEtapa_Template]') AND parent_object_id = OBJECT_ID(N'[dbo].[FlowEtapa]'))
BEGIN
    ALTER TABLE dbo.FlowEtapa
    ADD CONSTRAINT FK_FlowEtapa_Template FOREIGN KEY (TemplateId) 
    REFERENCES dbo.Template (Id);
END
GO

-- Auto-relacionamento idempotente para a próxima etapa do fluxo
IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_FlowEtapa_ProximaEtapa]') AND parent_object_id = OBJECT_ID(N'[dbo].[FlowEtapa]'))
BEGIN
    ALTER TABLE dbo.FlowEtapa
    ADD CONSTRAINT FK_FlowEtapa_ProximaEtapa FOREIGN KEY (ProximaEtapaId) 
    REFERENCES dbo.FlowEtapa (Id);
END
GO

-- Criação idempotente do índice para performance no FlowId
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = N'IX_FlowEtapa_FlowId' AND object_id = OBJECT_ID(N'[dbo].[FlowEtapa]'))
BEGIN
    CREATE NONCLUSTERED INDEX IX_FlowEtapa_FlowId ON dbo.FlowEtapa (FlowId);
END
GO

-- Criação idempotente do índice para performance na árvore de decisão (ProximaEtapaId)
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = N'IX_FlowEtapa_ProximaEtapaId' AND object_id = OBJECT_ID(N'[dbo].[FlowEtapa]'))
BEGIN
    CREATE NONCLUSTERED INDEX IX_FlowEtapa_ProximaEtapaId ON dbo.FlowEtapa (ProximaEtapaId);
END
GO