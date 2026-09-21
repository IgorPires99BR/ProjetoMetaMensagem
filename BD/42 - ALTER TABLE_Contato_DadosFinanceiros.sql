USE ContactSolutionDB;
GO

-- Unifica o que seria uma entidade "Cliente" separada dentro do proprio Contato: quem
-- revende a plataforma pros proprios clientes de cobranca (ex: Sebrecon) cadastra o
-- contato (numero de WhatsApp) ja com os dados financeiros dele, sem tabela a parte.
-- Nome vira NomeContato (nome de quem atende o numero) e ganha NomeCliente (nome de quem
-- deve a fatura) -- podem ser pessoas diferentes.
IF EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[Contato]') AND name = 'Nome')
   AND NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[Contato]') AND name = 'NomeContato')
BEGIN
    EXEC sp_rename 'dbo.Contato.Nome', 'NomeContato', 'COLUMN';
END
GO

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[Contato]') AND name = 'NomeCliente')
    ALTER TABLE Contato ADD NomeCliente NVARCHAR(255) NULL;
GO

-- Padrao de negocio atual: vencimento dia 20, 2% de juros, 2% de juros ao mes, R$405,00 de
-- fatura -- mesmos defaults que estavam na entidade Cliente removida.
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[Contato]') AND name = 'DiaVencimento')
    ALTER TABLE Contato ADD DiaVencimento INT NULL;
GO

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[Contato]') AND name = 'TaxaJuros')
    ALTER TABLE Contato ADD TaxaJuros DECIMAL(5,2) NULL;
GO

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[Contato]') AND name = 'TaxaJurosMensal')
    ALTER TABLE Contato ADD TaxaJurosMensal DECIMAL(5,2) NULL;
GO

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[Contato]') AND name = 'ValorFatura')
    ALTER TABLE Contato ADD ValorFatura DECIMAL(10,2) NULL;
GO

-- EmpresaId direto: ate aqui o vinculo com a empresa so existia via UsuarioId -> Usuario.EmpresaId
-- (JOIN em toda consulta). Passa a ser coluna propria, populada a partir do dono atual do
-- contato, pra simplificar as consultas e bater com o pedido explicito do Igor.
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[Contato]') AND name = 'EmpresaId')
    ALTER TABLE Contato ADD EmpresaId UNIQUEIDENTIFIER NULL;
GO

UPDATE c
SET c.EmpresaId = u.EmpresaId
FROM Contato c
INNER JOIN Usuario u ON u.Id = c.UsuarioId
WHERE c.EmpresaId IS NULL;
GO

IF EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[Contato]') AND name = 'EmpresaId' AND is_nullable = 1)
    ALTER TABLE Contato ALTER COLUMN EmpresaId UNIQUEIDENTIFIER NOT NULL;
GO

IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_Contato_Empresa')
    ALTER TABLE Contato ADD CONSTRAINT FK_Contato_Empresa FOREIGN KEY (EmpresaId) REFERENCES Empresa(Id);
GO
