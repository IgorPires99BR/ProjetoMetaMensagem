-- 1. Tabela 'companies'
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[companies]') AND type in (N'U'))
BEGIN
    CREATE TABLE companies (
        id NVARCHAR(450) PRIMARY KEY,
        name NVARCHAR(255) NOT NULL,
        email NVARCHAR(255),
        phone NVARCHAR(50),
        bot_whatsapp NVARCHAR(MAX),
        created_at DATETIMEOFFSET DEFAULT SYSDATETIMEOFFSET()
    );
END
GO

-- 2. Tabela 'flows'
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[flows]') AND type in (N'U'))
BEGIN
    CREATE TABLE flows (
        id NVARCHAR(450) PRIMARY KEY,
        company_id NVARCHAR(450),
        name NVARCHAR(255) NOT NULL,
        messages NVARCHAR(MAX),
        updated_at DATETIMEOFFSET DEFAULT SYSDATETIMEOFFSET(),
        
        CONSTRAINT CK_flows_messages_JSON CHECK (ISJSON(messages) > 0),
        CONSTRAINT FK_flows_companies FOREIGN KEY (company_id) REFERENCES companies(id)
    );
END
GO

-- 3. Tabela 'conversations'
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[conversations]') AND type in (N'U'))
BEGIN
    CREATE TABLE conversations (
        id INT IDENTITY(1,1) PRIMARY KEY,
        company_id NVARCHAR(450),
        phone NVARCHAR(50),
        status_funil NVARCHAR(100),
        status NVARCHAR(100),
        step NVARCHAR(100),
        nome NVARCHAR(255),
        email NVARCHAR(255),
        updated_at DATETIMEOFFSET DEFAULT SYSDATETIMEOFFSET(),

        CONSTRAINT FK_conversations_companies FOREIGN KEY (company_id) REFERENCES companies(id)
    );
END
GO

-- 4. Tabela 'messages'
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[messages]') AND type in (N'U'))
BEGIN
    CREATE TABLE messages (
        id INT IDENTITY(1,1) PRIMARY KEY,
        company_id NVARCHAR(450),
        phone NVARCHAR(50),
        direction NVARCHAR(50),
        text NVARCHAR(MAX),
        created_at DATETIMEOFFSET DEFAULT SYSDATETIMEOFFSET(),

        CONSTRAINT FK_messages_companies FOREIGN KEY (company_id) REFERENCES companies(id)
    );
END
GO