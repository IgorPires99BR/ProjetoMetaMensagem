USE ContactSolutionDB
GO

IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[ConversationState]') AND type in (N'U'))
BEGIN
    CREATE TABLE dbo.ConversationState (
        Id UNIQUEIDENTIFIER NOT NULL DEFAULT NEWID(),
        EmpresaId UNIQUEIDENTIFIER NOT NULL,
        ContatoId UNIQUEIDENTIFIER NOT NULL,
        FlowId UNIQUEIDENTIFIER NOT NULL,
        EtapaAtualId UNIQUEIDENTIFIER NULL,
        Variaveis NVARCHAR(MAX) NULL,  -- JSON com variaveis capturadas (ex: {"nome":"Joao"})
        DataInicio DATETIME NOT NULL DEFAULT GETDATE(),
        DataAtualizacao DATETIME NULL,
        Finalizado BIT NOT NULL DEFAULT 0,

        CONSTRAINT PK_ConversationState PRIMARY KEY CLUSTERED (Id)
    );
END
GO

-- FK com Empresa
IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_ConvState_Empresa]') AND parent_object_id = OBJECT_ID(N'[dbo].[ConversationState]'))
BEGIN
    ALTER TABLE dbo.ConversationState
    ADD CONSTRAINT FK_ConvState_Empresa FOREIGN KEY (EmpresaId)
    REFERENCES dbo.Empresa (Id);
END
GO

-- FK com Contato
IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_ConvState_Contato]') AND parent_object_id = OBJECT_ID(N'[dbo].[ConversationState]'))
BEGIN
    ALTER TABLE dbo.ConversationState
    ADD CONSTRAINT FK_ConvState_Contato FOREIGN KEY (ContatoId)
    REFERENCES dbo.Contato (Id);
END
GO

-- FK com Flow
IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_ConvState_Flow]') AND parent_object_id = OBJECT_ID(N'[dbo].[ConversationState]'))
BEGIN
    ALTER TABLE dbo.ConversationState
    ADD CONSTRAINT FK_ConvState_Flow FOREIGN KEY (FlowId)
    REFERENCES dbo.Flow (Id);
END
GO

-- FK com FlowEtapa (nullable)
IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_ConvState_Etapa]') AND parent_object_id = OBJECT_ID(N'[dbo].[ConversationState]'))
BEGIN
    ALTER TABLE dbo.ConversationState
    ADD CONSTRAINT FK_ConvState_Etapa FOREIGN KEY (EtapaAtualId)
    REFERENCES dbo.FlowEtapa (Id);
END
GO

-- Indice para busca por empresa + contato
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = N'IX_ConvState_Empresa_Contato' AND object_id = OBJECT_ID(N'[dbo].[ConversationState]'))
BEGIN
    CREATE NONCLUSTERED INDEX IX_ConvState_Empresa_Contato ON dbo.ConversationState (EmpresaId, ContatoId);
END
GO

-- Indice para listar conversas ativas de um flow
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = N'IX_ConvState_FlowId' AND object_id = OBJECT_ID(N'[dbo].[ConversationState]'))
BEGIN
    CREATE NONCLUSTERED INDEX IX_ConvState_FlowId ON dbo.ConversationState (FlowId);
END
GO
