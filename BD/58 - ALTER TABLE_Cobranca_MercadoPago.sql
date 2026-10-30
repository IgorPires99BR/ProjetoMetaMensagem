USE ContactSolutionDB;
GO

-- Mercado Pago passa a gerar os Pix das cobrancas (orders na conta da Contact Solution); o Asaas
-- fica de reserva. Cada cobranca guarda quem gerou o Pix: a baixa e o cancelamento precisam
-- voltar no mesmo provedor depois que o ativo mudar. Nulo = Asaas (todas as anteriores).
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[CobrancaCliente]') AND name = 'Provedor')
    ALTER TABLE CobrancaCliente ADD Provedor NVARCHAR(20) NULL;
GO

-- ticket_url da order: pagina de pagamento do proprio Mercado Pago.
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[CobrancaCliente]') AND name = 'LinkPagamentoExterno')
    ALTER TABLE CobrancaCliente ADD LinkPagamentoExterno NVARCHAR(500) NULL;
GO
