USE ContactSolutionDB;
GO

-- Producao ficou para tras: os scripts 07, 08, 09, 10, 12 e 13 nunca tinham sido aplicados la,
-- entao Tag/ContatoTag/Campanha/CampanhaContato/Produto/EstadoConversa/ConfiguracaoWebhook nao
-- existiam. Este script cria essas 7 tabelas com o mesmo conteudo dos scripts originais, ja com
-- os nomes em portugues (script 21) e as FKs para Fluxo/FluxoEtapa (ja renomeadas em producao).

-- 1. Tag
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Tag]') AND type in (N'U'))
BEGIN
    CREATE TABLE Tag (
        Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
        EmpresaId UNIQUEIDENTIFIER NOT NULL,
        Nome NVARCHAR(100) NOT NULL,
        Cor NVARCHAR(7) DEFAULT '#3D6EE8',
        DataCriacao DATETIME DEFAULT GETDATE(),

        CONSTRAINT FK_Tag_Empresa FOREIGN KEY (EmpresaId) REFERENCES Empresa(Id)
    );
END
GO

-- 2. ContatoTag
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[ContatoTag]') AND type in (N'U'))
BEGIN
    CREATE TABLE ContatoTag (
        ContatoId UNIQUEIDENTIFIER NOT NULL,
        TagId UNIQUEIDENTIFIER NOT NULL,
        DataCriacao DATETIME DEFAULT GETDATE(),

        CONSTRAINT PK_ContatoTag PRIMARY KEY (ContatoId, TagId),
        CONSTRAINT FK_ContatoTag_Contato FOREIGN KEY (ContatoId) REFERENCES Contato(Id),
        CONSTRAINT FK_ContatoTag_Tag FOREIGN KEY (TagId) REFERENCES Tag(Id)
    );
END
GO

-- 3. Campanha / CampanhaContato
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='Campanha' AND xtype='U')
BEGIN
    CREATE TABLE Campanha (
        Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
        EmpresaId UNIQUEIDENTIFIER NOT NULL,
        Nome NVARCHAR(200) NOT NULL,
        TemplateId UNIQUEIDENTIFIER NULL,
        ConteudoLivre NVARCHAR(MAX) NULL,
        DataAgendamento DATETIME NOT NULL,
        Status NVARCHAR(50) DEFAULT 'AGENDADA',
        TotalContatos INT DEFAULT 0,
        Processados INT DEFAULT 0,
        DataCriacao DATETIME DEFAULT GETDATE(),
        CONSTRAINT FK_Campanha_Empresa FOREIGN KEY (EmpresaId) REFERENCES Empresa(Id)
    );
END
GO

IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='CampanhaContato' AND xtype='U')
BEGIN
    CREATE TABLE CampanhaContato (
        Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
        CampanhaId UNIQUEIDENTIFIER NOT NULL,
        ContatoId UNIQUEIDENTIFIER NOT NULL,
        Processado BIT DEFAULT 0,
        Sucesso BIT NULL,
        MensagemErro NVARCHAR(MAX) NULL,
        CONSTRAINT FK_CampanhaContato_Campanha FOREIGN KEY (CampanhaId) REFERENCES Campanha(Id),
        CONSTRAINT FK_CampanhaContato_Contato FOREIGN KEY (ContatoId) REFERENCES Contato(Id)
    );
END
GO

-- 4. Produto
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Produto]') AND type in (N'U'))
BEGIN
    CREATE TABLE Produto (
        Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
        EmpresaId UNIQUEIDENTIFIER NOT NULL,
        Nome NVARCHAR(255) NOT NULL,
        Descricao NVARCHAR(1000) NULL,
        Preco DECIMAL(18, 2) NOT NULL,
        ImagemUrl NVARCHAR(500) NULL,
        LinkUrl NVARCHAR(500) NULL,
        Categoria NVARCHAR(100) NULL,
        Ativo BIT NOT NULL DEFAULT 1,
        DataCriacao DATETIME NOT NULL DEFAULT GETDATE(),
        CONSTRAINT FK_Produto_Empresa FOREIGN KEY (EmpresaId) REFERENCES Empresa(Id)
    );
END
GO

-- 5. ConfiguracaoWebhook (nome ja traduzido -- script 12 original criava como WebhookConfig)
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[ConfiguracaoWebhook]') AND type in (N'U'))
BEGIN
    CREATE TABLE ConfiguracaoWebhook (
        Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
        EmpresaId UNIQUEIDENTIFIER NOT NULL,
        Nome NVARCHAR(255) NOT NULL,
        Url NVARCHAR(500) NOT NULL,
        Evento NVARCHAR(100) NOT NULL,
        TokenSegredo NVARCHAR(500) NULL,
        Ativo BIT NOT NULL DEFAULT 1,
        DataCriacao DATETIME NOT NULL DEFAULT GETDATE(),
        CONSTRAINT FK_ConfiguracaoWebhook_Empresa FOREIGN KEY (EmpresaId) REFERENCES Empresa(Id)
    );
END
GO

-- 6. EstadoConversa (nome ja traduzido -- script 07 original criava como ConversationState;
-- as FKs abaixo ja apontam para Fluxo/FluxoEtapa, que sao os nomes atuais em producao)
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[EstadoConversa]') AND type in (N'U'))
BEGIN
    CREATE TABLE dbo.EstadoConversa (
        Id UNIQUEIDENTIFIER NOT NULL DEFAULT NEWID(),
        EmpresaId UNIQUEIDENTIFIER NOT NULL,
        ContatoId UNIQUEIDENTIFIER NOT NULL,
        FlowId UNIQUEIDENTIFIER NOT NULL,
        EtapaAtualId UNIQUEIDENTIFIER NULL,
        Variaveis NVARCHAR(MAX) NULL,
        DataInicio DATETIME NOT NULL DEFAULT GETDATE(),
        DataAtualizacao DATETIME NULL,
        Finalizado BIT NOT NULL DEFAULT 0,

        CONSTRAINT PK_EstadoConversa PRIMARY KEY CLUSTERED (Id)
    );
END
GO

IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_EstadoConversa_Empresa]') AND parent_object_id = OBJECT_ID(N'[dbo].[EstadoConversa]'))
BEGIN
    ALTER TABLE dbo.EstadoConversa
    ADD CONSTRAINT FK_EstadoConversa_Empresa FOREIGN KEY (EmpresaId)
    REFERENCES dbo.Empresa (Id);
END
GO

IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_EstadoConversa_Contato]') AND parent_object_id = OBJECT_ID(N'[dbo].[EstadoConversa]'))
BEGIN
    ALTER TABLE dbo.EstadoConversa
    ADD CONSTRAINT FK_EstadoConversa_Contato FOREIGN KEY (ContatoId)
    REFERENCES dbo.Contato (Id);
END
GO

IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_EstadoConversa_Fluxo]') AND parent_object_id = OBJECT_ID(N'[dbo].[EstadoConversa]'))
BEGIN
    ALTER TABLE dbo.EstadoConversa
    ADD CONSTRAINT FK_EstadoConversa_Fluxo FOREIGN KEY (FlowId)
    REFERENCES dbo.Fluxo (Id);
END
GO

IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_EstadoConversa_Etapa]') AND parent_object_id = OBJECT_ID(N'[dbo].[EstadoConversa]'))
BEGIN
    ALTER TABLE dbo.EstadoConversa
    ADD CONSTRAINT FK_EstadoConversa_Etapa FOREIGN KEY (EtapaAtualId)
    REFERENCES dbo.FluxoEtapa (Id);
END
GO

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = N'IX_EstadoConversa_Empresa_Contato' AND object_id = OBJECT_ID(N'[dbo].[EstadoConversa]'))
BEGIN
    CREATE NONCLUSTERED INDEX IX_EstadoConversa_Empresa_Contato ON dbo.EstadoConversa (EmpresaId, ContatoId);
END
GO

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = N'IX_EstadoConversa_FlowId' AND object_id = OBJECT_ID(N'[dbo].[EstadoConversa]'))
BEGIN
    CREATE NONCLUSTERED INDEX IX_EstadoConversa_FlowId ON dbo.EstadoConversa (FlowId);
END
GO
