USE ContactSolutionDB;
GO

BEGIN TRANSACTION;

-- TokenExpiraEm: validade do SystemUserToken quando trocado por long-lived (~60 dias) logo
-- apos o Embedded Signup. NULL = token sem expiracao conhecida (numeros cadastrados antes
-- dessa troca existir, ou quando a troca por long-lived falhou e ficou o token curto mesmo).
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[Numero]') AND name = 'TokenExpiraEm')
BEGIN
    ALTER TABLE Numero ADD TokenExpiraEm DATETIME NULL;
END

COMMIT TRANSACTION;
GO
