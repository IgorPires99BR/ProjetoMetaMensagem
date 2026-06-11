use ContactSolutionDB

IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Flow]') AND type in (N'U'))
BEGIN
    CREATE TABLE dbo.Flow (
        Id UNIQUEIDENTIFIER NOT NULL DEFAULT NEWID(),
        EmpresaId UNIQUEIDENTIFIER NOT NULL,
        Nome NVARCHAR(150) NOT NULL,
        Descricao NVARCHAR(500) NULL,
        Ativo BIT NOT NULL DEFAULT 1,
        DataCriacao DATETIME NOT NULL DEFAULT GETDATE(),
        
        CONSTRAINT PK_Flow PRIMARY KEY CLUSTERED (Id)
    );
END
GO

-- Garantia de Idempotência para a FK com a tabela Empresa
IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_Flow_Empresa]') AND parent_object_id = OBJECT_ID(N'[dbo].[Flow]'))
BEGIN
    ALTER TABLE dbo.Flow
    ADD CONSTRAINT FK_Flow_Empresa FOREIGN KEY (EmpresaId) 
    REFERENCES dbo.Empresa (Id) ON DELETE CASCADE;
END
GO