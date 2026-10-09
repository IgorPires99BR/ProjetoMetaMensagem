USE ContactSolutionDB;
GO

-- Conta onde a empresa recebe o Pix das cobrancas que faz aos PROPRIOS clientes (CobrancaCliente,
-- BD/48). O dinheiro cai direto na conta da empresa, nunca passa pela Contact Solution: por isso
-- uma linha por empresa, com as credenciais da API Pix do Itau dela.
--
-- ClientSecret e a chave privada do certificado ficam criptografados (AES-GCM, chave em
-- Criptografia:Chave na configuracao da API): com eles da pra consultar e DEVOLVER Pix recebidos
-- na conta do cliente. O certificado (parte publica) fica em texto, so pra mostrar validade/titular.
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='DadosBancariosEmpresa' AND xtype='U')
BEGIN
    CREATE TABLE DadosBancariosEmpresa (
        EmpresaId UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
        Banco NVARCHAR(3) NOT NULL DEFAULT ('341'),
        Agencia NVARCHAR(10) NULL,
        Conta NVARCHAR(20) NULL,
        ContaDigito NVARCHAR(2) NULL,
        TitularNome NVARCHAR(255) NULL,
        TitularDocumento NVARCHAR(14) NULL,
        TipoChavePix NVARCHAR(20) NULL,
        ChavePix NVARCHAR(100) NULL,
        Ambiente NVARCHAR(20) NOT NULL DEFAULT ('SANDBOX'),
        ClientId NVARCHAR(100) NULL,
        ClientSecretCriptografado NVARCHAR(MAX) NULL,
        CertificadoPem NVARCHAR(MAX) NULL,
        ChavePrivadaCriptografada NVARCHAR(MAX) NULL,
        CertificadoValidoAte DATETIME NULL,
        CertificadoTitular NVARCHAR(500) NULL,
        CobrancaPixAtiva BIT NOT NULL DEFAULT (0),
        DataCriacao DATETIME NOT NULL DEFAULT (GETDATE()),
        DataAtualizacao DATETIME NULL,

        -- CASCADE: sem a empresa esses dados nao servem pra nada, e sem isso excluir uma empresa
        -- que so tinha cadastrado a conta passaria a falhar por FK.
        CONSTRAINT FK_DadosBancariosEmpresa_Empresa FOREIGN KEY (EmpresaId) REFERENCES Empresa(Id) ON DELETE CASCADE
    );
END
GO
