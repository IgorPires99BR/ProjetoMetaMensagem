USE ContactSolutionDB;
GO

-- Script de Migração/Alteração para bases já existentes
IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Empresa]') AND type in (N'U'))
BEGIN
    IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[Empresa]') AND name = 'MetaAccessToken')
        ALTER TABLE Empresa ADD MetaAccessToken NVARCHAR(MAX) NULL;

    IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[Empresa]') AND name = 'AppIdMeta')
        ALTER TABLE Empresa ADD AppIdMeta NVARCHAR(100) NULL;

    IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[Empresa]') AND name = 'StatusAccount')
        ALTER TABLE Empresa ADD StatusAccount NVARCHAR(50) DEFAULT 'Ativo';

    IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[Empresa]') AND name = 'TokenWebhookLocal')
        ALTER TABLE Empresa ADD TokenWebhookLocal UNIQUEIDENTIFIER DEFAULT NEWID();

    IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[Empresa]') AND name = 'PlanoId')
        ALTER TABLE Empresa ADD PlanoId NVARCHAR(50) DEFAULT 'Bronze';
END
GO