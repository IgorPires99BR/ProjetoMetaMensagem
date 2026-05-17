USE ContactSolutionDB;
GO

BEGIN TRANSACTION;

-- 1. Adicionando WabaId na tabela Empresa
-- Necessário para vincular a conta business e listar os números
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[Empresa]') AND name = 'WabaId')
BEGIN
    ALTER TABLE Empresa ADD WabaId NVARCHAR(100) NULL;
END

-- 2. Adicionando campos de status e saúde na tabela Numero
-- StatusMeta: CONNECTED, PENDING, DELETED, FLAGGED, etc.
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[Numero]') AND name = 'StatusMeta')
BEGIN
    ALTER TABLE Numero ADD StatusMeta NVARCHAR(50) NULL;
END

-- QualidadeMeta: GREEN (High), YELLOW (Medium), RED (Low), UNKNOWN
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[Numero]') AND name = 'QualidadeMeta')
BEGIN
    ALTER TABLE Numero ADD QualidadeMeta NVARCHAR(50) NULL;
END

COMMIT TRANSACTION;
GO