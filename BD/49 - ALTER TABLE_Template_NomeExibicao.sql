USE ContactSolutionDB;
GO

-- Nome do template dentro do sistema, editavel a qualquer momento. NomeTemplate continua sendo
-- o nome tecnico na Meta, que ela nao deixa renomear -- e por ele que o disparo identifica o
-- template e que a sincronizacao casa os legados, entao nao pode mudar. NULL = usa NomeTemplate.
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[Template]') AND name = 'NomeExibicao')
    ALTER TABLE Template ADD NomeExibicao NVARCHAR(255) NULL;
GO
