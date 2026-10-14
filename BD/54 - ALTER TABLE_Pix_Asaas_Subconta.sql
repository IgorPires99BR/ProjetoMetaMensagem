USE ContactSolutionDB;
GO

-- Troca da Transfeera (BD/53) pelo Asaas. O Pix continua sendo gerado na conta da PLATAFORMA,
-- mas o split do Asaas so repassa para outra conta Asaas: cada empresa ganha uma SUBCONTA,
-- criada pela API (POST /v3/accounts), e o repasse vai para o walletId dela. TipoChavePix e
-- ChavePix ficam sem uso (o saque sai da subconta, pelo app/painel do Asaas).
--
-- A apiKey da subconta so aparece na resposta da criacao: fica criptografada (AES-GCM, mesma
-- Criptografia:Chave do BD/51) para consultar a situacao/documentos da subconta depois.
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[DadosBancariosEmpresa]') AND name = 'AsaasWalletId')
    ALTER TABLE DadosBancariosEmpresa ADD
        AsaasContaId NVARCHAR(50) NULL,
        AsaasWalletId NVARCHAR(50) NULL,
        AsaasApiKeyCriptografada NVARCHAR(MAX) NULL,
        AsaasContaCriadaEm DATETIME NULL;
GO

-- Id da cobranca no Asaas (pay_...). O Txid continua sendo o Id da CobrancaCliente sem hifens:
-- vai no externalReference do Asaas e no link da pagina do Pix -- o webhook casa por ele.
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[CobrancaCliente]') AND name = 'PixIdExterno')
    ALTER TABLE CobrancaCliente ADD PixIdExterno NVARCHAR(50) NULL;
GO

-- O Asaas so cria cobranca para um cliente (customer) com CPF/CNPJ. So digitos.
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[Contato]') AND name = 'CpfCnpj')
    ALTER TABLE Contato ADD CpfCnpj NVARCHAR(14) NULL;
GO

-- Quem estava com a cobranca ativa pela chave Pix (Transfeera) ainda nao tem subconta: o Pix
-- nao sairia. Desativa ate a subconta ser criada na tela de dados bancarios.
UPDATE DadosBancariosEmpresa SET CobrancaPixAtiva = 0, DataAtualizacao = GETDATE()
WHERE CobrancaPixAtiva = 1 AND AsaasWalletId IS NULL;
GO
