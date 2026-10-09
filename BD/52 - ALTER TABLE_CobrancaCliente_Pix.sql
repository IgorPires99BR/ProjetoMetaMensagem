USE ContactSolutionDB;
GO

-- Pix do Itau gerado por cobranca (conta da propria empresa, ver BD/51). O Txid e o Id da
-- CobrancaCliente sem hifens: e por ele que a baixa casa o pagamento (webhook ou consulta
-- periodica), sempre reconsultando o Itau antes -- o txid aparece no link enviado ao cliente.
--
-- ValorCobrado = Valor + Multa + Juros no momento do disparo (multa unica e juros pro rata dia
-- so depois do vencimento). Valor continua sendo o snapshot da fatura, sem encargos.
-- NULL em tudo = cobranca sem Pix (empresa sem cobranca Pix ativa), como antes.
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[CobrancaCliente]') AND name = 'Txid')
    ALTER TABLE CobrancaCliente ADD
        Txid NVARCHAR(35) NULL,
        PixCopiaECola NVARCHAR(512) NULL,
        PixExpiraEm DATETIME NULL,
        Multa DECIMAL(10,2) NULL,
        Juros DECIMAL(10,2) NULL,
        ValorCobrado DECIMAL(10,2) NULL,
        ValorPago DECIMAL(10,2) NULL,
        EndToEndId NVARCHAR(50) NULL;
GO

-- Consulta periodica (pendentes com Pix ainda dentro da validade) e baixa pelo webhook (por txid).
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_CobrancaCliente_Txid' AND object_id = OBJECT_ID(N'[dbo].[CobrancaCliente]'))
    CREATE INDEX IX_CobrancaCliente_Txid ON CobrancaCliente (Txid) WHERE Txid IS NOT NULL;
GO
