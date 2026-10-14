USE ContactSolutionDB;
GO

-- Troca da integracao direta com o Itau (Pix na conta da propria empresa, ver BD/51) pela
-- Transfeera: o Pix e gerado na conta da PLATAFORMA e o split repassa a parte da empresa para
-- a chave Pix dela. A plataforma fica com a comissao.
--
-- As colunas do Itau em DadosBancariosEmpresa (Banco, Agencia, Conta, ContaDigito, Titular*,
-- Ambiente, ClientId, ClientSecretCriptografado, CertificadoPem, ChavePrivadaCriptografada,
-- Certificado*) ficam sem uso, mas nao sao removidas aqui: se for preciso voltar ao Itau, os
-- dados continuam la. Remover em uma migration futura, depois da Transfeera validada em producao.

-- Comissao da plataforma por empresa, definida so pelo admin da plataforma.
-- ComissaoTipo: 'PERCENTUAL' (ComissaoValor = % do valor cobrado) ou 'FIXA' (ComissaoValor = R$).
-- NULL = ainda nao combinada: a cobranca Pix nao pode ser ativada.
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[DadosBancariosEmpresa]') AND name = 'ComissaoTipo')
    ALTER TABLE DadosBancariosEmpresa ADD
        ComissaoTipo NVARCHAR(20) NULL,
        ComissaoValor DECIMAL(10,2) NULL;
GO

-- Split de cada cobranca no momento do disparo: ValorComissao + ValorRepasse = ValorCobrado.
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[CobrancaCliente]') AND name = 'ValorComissao')
    ALTER TABLE CobrancaCliente ADD
        ValorComissao DECIMAL(10,2) NULL,
        ValorRepasse DECIMAL(10,2) NULL;
GO

-- Pix gerado com credencial do Itau nao pode mais ser conferido (a consulta agora vai na
-- Transfeera, que nao conhece esse txid): desativa a cobranca de quem estava no Itau para que
-- o admin revise a chave Pix e a plataforma defina a comissao antes de reativar.
UPDATE DadosBancariosEmpresa SET CobrancaPixAtiva = 0, DataAtualizacao = GETDATE()
WHERE CobrancaPixAtiva = 1 AND ComissaoTipo IS NULL;
GO
