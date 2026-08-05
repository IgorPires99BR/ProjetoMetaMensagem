USE ContactSolutionDB;
GO

-- Corrige gap de schema: EmpresaRepository sempre leu/gravou Empresa.PhoneNumberId e Empresa.WabaId,
-- mas nenhum script anterior criou essas colunas na tabela Empresa (WabaId ali é distinto do WabaID gravado em Numero).
-- Isso quebrava 100% dos disparos de template e o lookup de empresa por PhoneNumberId no webhook.
IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Empresa]') AND type in (N'U'))
BEGIN
    IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[Empresa]') AND name = 'PhoneNumberId')
        ALTER TABLE Empresa ADD PhoneNumberId NVARCHAR(100) NULL;

    IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[Empresa]') AND name = 'WabaId')
        ALTER TABLE Empresa ADD WabaId NVARCHAR(100) NULL;
END
GO
