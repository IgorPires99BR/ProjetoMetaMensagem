USE ContactSolutionDB;
GO

-- Marca que disparar este template deve abrir uma CobrancaCliente (ver BD/48) -- cobrança do
-- cliente FINAL de quem revende a plataforma (ex: Sebrecon cobrando os devedores dela), nao a
-- Assinatura SaaS da propria Contact Solution (essa e outra tabela, alimentada pela Cakto).
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[Template]') AND name = 'GeraCobranca')
    ALTER TABLE Template ADD GeraCobranca BIT NOT NULL DEFAULT (0);
GO
