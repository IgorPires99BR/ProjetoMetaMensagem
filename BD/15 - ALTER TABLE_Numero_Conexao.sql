USE ContactSolutionDB;
GO

BEGIN TRANSACTION;

-- 1. TipoConexao: 1 = ApiOficial (padrão atual/registro clássico), 2 = Coexistencia
-- DEFAULT 1 preserva o comportamento dos registros já existentes.
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[Numero]') AND name = 'TipoConexao')
BEGIN
    ALTER TABLE Numero ADD TipoConexao INT NOT NULL DEFAULT 1;
END

-- 2. StatusConexao: Pendente, Conectado, Erro, Desconectado
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[Numero]') AND name = 'StatusConexao')
BEGIN
    ALTER TABLE Numero ADD StatusConexao NVARCHAR(50) NULL;
END

-- 3. SystemUserToken: token de sistema obtido via troca de "code" do Embedded Signup
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[Numero]') AND name = 'SystemUserToken')
BEGIN
    ALTER TABLE Numero ADD SystemUserToken NVARCHAR(MAX) NULL;
END

-- 4. WabaId: override opcional por número. O fluxo padrão continua usando Empresa.WabaId;
-- esta coluna só é usada quando o Embedded Signup retornar um WABA diferente do vinculado à Empresa.
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[Numero]') AND name = 'WabaId')
BEGIN
    ALTER TABLE Numero ADD WabaId NVARCHAR(100) NULL;
END

-- 5. DataUltimaSincronizacao: usado no fluxo CoEx para saber quando o número foi sincronizado pela última vez
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[Numero]') AND name = 'DataUltimaSincronizacao')
BEGIN
    ALTER TABLE Numero ADD DataUltimaSincronizacao DATETIME NULL;
END

COMMIT TRANSACTION;
GO
