USE ContactSolutionDB;
GO

-- 1. Tabela 'Empresa'
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Empresa]') AND type in (N'U'))
BEGIN
    CREATE TABLE Empresa (
        Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
        Nome NVARCHAR(255) NOT NULL,
        Email NVARCHAR(255),
        WabaId NVARCHAR(100),       -- Mantido (ID da Conta Comercial do WhatsApp)
        Cnpj NVARCHAR(20),
        Telefone NVARCHAR(50),
        DataCriacao DATETIME DEFAULT GETDATE(),
        DataAtualizacao DATETIME NULL,

        -- 🚀 NOVOS CAMPOS ADICIONADOS:
        MetaAccessToken NVARCHAR(MAX) NULL,             -- Armazena o token permanente (System User Token) de cada cliente
        AppIdMeta NVARCHAR(100) NULL,                   -- ID do App da Meta vinculado (útil para rastreamento e segurança)
        StatusAccount NVARCHAR(50) DEFAULT 'Ativo',     -- Controle administrativo do SaaS (Ex: Ativo, Suspenso, Inadimplente)
        TokenWebhookLocal UNIQUEIDENTIFIER DEFAULT NEWID(), -- GUID único por tenant para isolar e validar as rotas de Webhook de entrada
        PlanoId NVARCHAR(50) DEFAULT 'Bronze'           -- Define o limite e os recursos de disparos no C# (Ex: Bronze, Prata, Ouro)
    );
END
GO

-- 2. Tabela 'Usuario'
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Usuario]') AND type in (N'U'))
BEGIN
    CREATE TABLE Usuario (
        Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
        EmpresaId UNIQUEIDENTIFIER NOT NULL,
        Nome NVARCHAR(255) NOT NULL,
        Email NVARCHAR(255),
        SenhaHash NVARCHAR(MAX),
        IsAdmin BIT NOT NULL DEFAULT 0, -- Booleano (0 = Falso, 1 = Verdadeiro)
        DataCriacao DATETIME DEFAULT GETDATE(),
        DataAtualizacao DATETIME NULL,

        CONSTRAINT FK_Usuario_Empresa FOREIGN KEY (EmpresaId) REFERENCES Empresa(Id)
    );
END
GO

-- 3. Tabela 'Numero'
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Numero]') AND type in (N'U'))
BEGIN
    CREATE TABLE Numero (
        Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
        UsuarioId UNIQUEIDENTIFIER NOT NULL,
        Telefone NVARCHAR(50) NOT NULL,
        Descricao NVARCHAR(100),
        InstanciaId NVARCHAR(255),
        StatusMeta NVARCHAR(50),    -- Ex: CONNECTED, PENDING, FLAGGED
        QualidadeMeta NVARCHAR(50), -- Ex: GREEN, YELLOW, RED
        DataCriacao DATETIME DEFAULT GETDATE(),
        DataAtualizacao DATETIME NULL,

        CONSTRAINT FK_Numero_Usuario FOREIGN KEY (UsuarioId) REFERENCES Usuario(Id)
    );
END
GO

-- 4. Tabela 'Contato'
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Contato]') AND type in (N'U'))
BEGIN
    CREATE TABLE Contato (
        Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
        UsuarioId UNIQUEIDENTIFIER NOT NULL,
        Telefone NVARCHAR(50) NOT NULL,
        Nome NVARCHAR(255),
        Email NVARCHAR(255),
        DataCriacao DATETIME DEFAULT GETDATE(),
        DataAtualizacao DATETIME NULL,

        CONSTRAINT FK_Contato_Usuario FOREIGN KEY (UsuarioId) REFERENCES Usuario(Id)
    );
END
GO

-- 5. Tabela 'Template'
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Template]') AND type in (N'U'))
BEGIN
    CREATE TABLE Template (
        Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
        EmpresaId UNIQUEIDENTIFIER NOT NULL,
        NomeTemplate NVARCHAR(255) NOT NULL,
        Conteudo NVARCHAR(MAX) NOT NULL,
        Categoria NVARCHAR(100),
        Idioma NVARCHAR(10) DEFAULT 'pt_BR',
        Status NVARCHAR(50),
        DataCriacao DATETIME DEFAULT GETDATE(),
        DataAtualizacao DATETIME NULL,
        
        CONSTRAINT FK_Template_Empresa FOREIGN KEY (EmpresaId) REFERENCES Empresa(Id)
    );
END
GO
