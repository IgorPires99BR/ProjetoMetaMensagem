use ContactSolutionDB

-- 1. Tabela 'Empresas'
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Empresas]') AND type in (N'U'))
BEGIN
    CREATE TABLE Empresas (
        Id NVARCHAR(450) PRIMARY KEY,
        Nome NVARCHAR(255) NOT NULL,
        Email NVARCHAR(255),
        Telefone NVARCHAR(50),
        DataCriacao DATETIMEOFFSET DEFAULT SYSDATETIMEOFFSET()
    );
END
GO

-- 2. Tabela 'Usuarios'
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Usuarios]') AND type in (N'U'))
BEGIN
    CREATE TABLE Usuarios (
        Id NVARCHAR(450) PRIMARY KEY,
        EmpresaId NVARCHAR(450) NOT NULL,
        Nome NVARCHAR(255) NOT NULL,
        Email NVARCHAR(255),
        SenhaHash NVARCHAR(MAX),
        DataCriacao DATETIMEOFFSET DEFAULT SYSDATETIMEOFFSET(),

        CONSTRAINT FK_Usuarios_Empresas FOREIGN KEY (EmpresaId) REFERENCES Empresas(Id)
    );
END
GO

-- 3. Tabela 'Numeros' (Relacionada ao Usuário)
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Numeros]') AND type in (N'U'))
BEGIN
    CREATE TABLE Numeros (
        Id INT IDENTITY(1,1) PRIMARY KEY,
        UsuarioINumeroTelefoned NVARCHAR(450) NOT NULL,
         NVARCHAR(50) NOT NULL,
        Descricao NVARCHAR(100), -- Ex: 'WhatsApp Pessoal', 'Comercial'
        InstanciaId NVARCHAR(255), -- Útil para integrações de API

        CONSTRAINT FK_Numeros_Usuarios FOREIGN KEY (UsuarioId) REFERENCES Usuarios(Id)
    );
END
GO

-- 4. Tabela 'Contatos'
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Contatos]') AND type in (N'U'))
BEGIN
    CREATE TABLE Contatos (
        Id INT IDENTITY(1,1) PRIMARY KEY,
        UsuarioId NVARCHAR(450) NOT NULL,
        Telefone NVARCHAR(50) NOT NULL, -- Único campo obrigatório conforme solicitado
        Nome NVARCHAR(255),
        Email NVARCHAR(255),
        DataCriacao DATETIMEOFFSET DEFAULT SYSDATETIMEOFFSET(),

        CONSTRAINT FK_Contatos_Usuarios FOREIGN KEY (UsuarioId) REFERENCES Usuarios(Id)
    );
END
GO

-- 5. Tabela 'Templates' (WhatsApp)
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Templates]') AND type in (N'U'))
BEGIN
    CREATE TABLE Templates (
        Id INT IDENTITY(1,1) PRIMARY KEY,
        EmpresaId NVARCHAR(450) NOT NULL,
        NomeTemplate NVARCHAR(255) NOT NULL,
        Conteudo NVARCHAR(MAX) NOT NULL,
        Categoria NVARCHAR(100), -- Ex: marketing, utility
        Idioma NVARCHAR(10) DEFAULT 'pt_BR',
        Status NVARCHAR(50), -- Ex: APPROVED, PENDING, REJECTED
        
        CONSTRAINT FK_Templates_Empresas FOREIGN KEY (EmpresaId) REFERENCES Empresas(Id)
    );
END
GO