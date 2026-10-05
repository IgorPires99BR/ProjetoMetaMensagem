USE ContactSolutionDB;
GO

-- Segundo numero do mesmo contato (ex: empresa com dois socios, marido e mulher): o disparo
-- sai para os dois numeros, mas o contato continua um so -- e a cobranca tambem (uma por
-- contato, nao por numero). NULL = contato com um numero so, como sempre foi.
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[Contato]') AND name = 'Telefone2')
    ALTER TABLE Contato ADD Telefone2 NVARCHAR(50) NULL;
GO
