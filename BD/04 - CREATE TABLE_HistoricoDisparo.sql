use ContactSolutionDB

IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[HistoricoDisparo]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[HistoricoDisparo](
        [Id] [uniqueidentifier] NOT NULL,
        [EmpresaId] [uniqueidentifier] NOT NULL,
        [ContatoId] [uniqueidentifier] NOT NULL,
        [TemplateId] [uniqueidentifier] NULL,
        [TipoDisparo] [nvarchar](20) NOT NULL,
        [Conteudo] [nvarchar](max) NOT NULL,
        [WamidMeta] [nvarchar](255) NOT NULL,
        [DataEnvio] [datetime] NOT NULL,
        
        CONSTRAINT [PK_HistoricoDisparo] PRIMARY KEY CLUSTERED ([Id] ASC)
    );
END
GO

-- 2. CRIAÇÃO DOS VALORES PADRÃO / CONSTRAINTS DEFAULT
-- Default para o Id (NEWID)
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[DF_HistoricoDisparo_Id]') AND type = 'D')
BEGIN
    ALTER TABLE [dbo].[HistoricoDisparo] 
    ADD CONSTRAINT [DF_HistoricoDisparo_Id] DEFAULT (NEWID()) FOR [Id];
END
GO

-- Default para a DataEnvio (GETDATE)
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[DF_HistoricoDisparo_DataEnvio]') AND type = 'D')
BEGIN
    ALTER TABLE [dbo].[HistoricoDisparo] 
    ADD CONSTRAINT [DF_HistoricoDisparo_DataEnvio] DEFAULT (GETDATE()) FOR [DataEnvio];
END
GO

-- 3. CRIAÇÃO DAS CHAVES ESTRANGEIRAS (FOREIGN KEYS)
-- FK com a tabela Empresa
IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_HistoricoDisparo_Empresa]') AND parent_object_id = OBJECT_ID(N'[dbo].[HistoricoDisparo]'))
BEGIN
    ALTER TABLE [dbo].[HistoricoDisparo] WITH CHECK ADD CONSTRAINT [FK_HistoricoDisparo_Empresa] 
    FOREIGN KEY([EmpresaId]) REFERENCES [dbo].[Empresa] ([Id]);
    
    ALTER TABLE [dbo].[HistoricoDisparo] CHECK CONSTRAINT [FK_HistoricoDisparo_Empresa];
END
GO

-- FK com a tabela Contato
IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_HistoricoDisparo_Contato]') AND parent_object_id = OBJECT_ID(N'[dbo].[HistoricoDisparo]'))
BEGIN
    ALTER TABLE [dbo].[HistoricoDisparo] WITH CHECK ADD CONSTRAINT [FK_HistoricoDisparo_Contato] 
    FOREIGN KEY([ContatoId]) REFERENCES [dbo].[Contato] ([Id]);
    
    ALTER TABLE [dbo].[HistoricoDisparo] CHECK CONSTRAINT [FK_HistoricoDisparo_Contato];
END
GO

-- FK com a tabela Template (Opcional/Nullable)
IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_HistoricoDisparo_Template]') AND parent_object_id = OBJECT_ID(N'[dbo].[HistoricoDisparo]'))
BEGIN
    ALTER TABLE [dbo].[HistoricoDisparo] WITH CHECK ADD CONSTRAINT [FK_HistoricoDisparo_Template] 
    FOREIGN KEY([TemplateId]) REFERENCES [dbo].[Template] ([Id]);
    
    ALTER TABLE [dbo].[HistoricoDisparo] CHECK CONSTRAINT [FK_HistoricoDisparo_Template];
END
GO

-- 4. CRIAÇÃO DOS ÍNDICES PARA PERFORMANCE
-- Índice para buscas por Empresa
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[dbo].[HistoricoDisparo]') AND name = N'IX_HistoricoDisparo_EmpresaId')
BEGIN
    CREATE NONCLUSTERED INDEX [IX_HistoricoDisparo_EmpresaId] ON [dbo].[HistoricoDisparo] ([EmpresaId] ASC);
END
GO

-- Índice para buscas por Contato (Timeline do cliente no CRM)
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[dbo].[HistoricoDisparo]') AND name = N'IX_HistoricoDisparo_ContatoId')
BEGIN
    CREATE NONCLUSTERED INDEX [IX_HistoricoDisparo_ContatoId] ON [dbo].[HistoricoDisparo] ([ContatoId] ASC);
END
GO

-- Índice Único para o ID da Meta (Garante integridade e performance na busca por Webhooks)
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[dbo].[HistoricoDisparo]') AND name = N'IX_HistoricoDisparo_WamidMeta')
BEGIN
    CREATE UNIQUE NONCLUSTERED INDEX [IX_HistoricoDisparo_WamidMeta] ON [dbo].[HistoricoDisparo] ([WamidMeta] ASC);
END
GO