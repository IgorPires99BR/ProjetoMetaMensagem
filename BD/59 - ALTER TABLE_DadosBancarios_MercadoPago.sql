USE ContactSolutionDB;
GO

-- Conta onde a EMPRESA recebe as cobrancas aos clientes dela: Mercado Pago (padrao) ou Asaas, so
-- uma. Com a conta conectada e a comissao definida, a cobranca sai em nome da empresa e a
-- plataforma fica so com a comissao (marketplace_fee no Mercado Pago, split no Asaas). Sem esses
-- dados, a cobranca e da propria Contact Solution, na conta dela.
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[DadosBancariosEmpresa]') AND name = 'ProvedorRecebimento')
    ALTER TABLE DadosBancariosEmpresa ADD ProvedorRecebimento NVARCHAR(20) NOT NULL
        CONSTRAINT DF_DadosBancariosEmpresa_ProvedorRecebimento DEFAULT ('MERCADOPAGO');
GO

-- Conta Mercado Pago da empresa, conectada por OAuth (a empresa autoriza o app da Contact
-- Solution no site do Mercado Pago). Tokens criptografados com a Criptografia:Chave do BD/51;
-- o access token vale 180 dias e e renovado por job antes de vencer.
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[DadosBancariosEmpresa]') AND name = 'MercadoPagoUserId')
    ALTER TABLE DadosBancariosEmpresa ADD
        MercadoPagoUserId NVARCHAR(50) NULL,
        MercadoPagoAccessTokenCriptografado NVARCHAR(MAX) NULL,
        MercadoPagoRefreshTokenCriptografado NVARCHAR(MAX) NULL,
        MercadoPagoTokenExpiraEm DATETIME NULL,
        MercadoPagoConectadoEm DATETIME NULL;
GO
