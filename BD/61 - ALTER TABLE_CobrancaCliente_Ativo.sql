USE ContactSolutionDB;
GO

-- Inativacao da cobranca (antiga ou acertada por fora): sai da tela e dos totais e nao gera mais
-- aviso de atraso nem recorrencia, mas continua no banco. Nao muda o Status: uma cobranca paga
-- continua paga. Status VENCIDA passa a ser gravado pelo envio de recorrencia, sem coluna nova.
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[CobrancaCliente]') AND name = 'Ativo')
    ALTER TABLE CobrancaCliente ADD Ativo BIT NOT NULL CONSTRAINT DF_CobrancaCliente_Ativo DEFAULT (1);
GO

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[CobrancaCliente]') AND name = 'DataInativacao')
    ALTER TABLE CobrancaCliente ADD DataInativacao DATETIME NULL;
GO
