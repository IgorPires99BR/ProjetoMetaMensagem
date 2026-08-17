USE ContactSolutionDB;
GO

-- De qual anuncio veio cada lead que chegou pelo WhatsApp.
--
-- Quando alguem toca num anuncio Click-to-WhatsApp, a Meta manda um objeto `referral` junto da
-- PRIMEIRA mensagem daquela pessoa -- e so na primeira. O campo mais importante e o `ctwa_clid`:
-- e ele que a Conversions API pede depois, quando a conversa vira venda, pra creditar a conversao
-- ao anuncio certo. Sem gravar na hora, o vinculo se perde e o Gerenciador de Anuncios nunca fica
-- sabendo que aquele lead comprou -- o algoritmo continua otimizando no escuro.
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'OrigemLead')
BEGIN
    CREATE TABLE dbo.OrigemLead (
        Id                  UNIQUEIDENTIFIER NOT NULL CONSTRAINT PK_OrigemLead PRIMARY KEY,
        EmpresaId           UNIQUEIDENTIFIER NOT NULL,
        ContatoId           UNIQUEIDENTIFIER NULL,
        Telefone            NVARCHAR(30)     NOT NULL,

        -- Identificador do clique no anuncio: o que a Conversions API consome
        CtwaClid            NVARCHAR(400)    NULL,
        -- Id do anuncio/post de origem e o tipo (ad, post)
        SourceId            NVARCHAR(120)    NULL,
        SourceType          NVARCHAR(40)     NULL,
        SourceUrl           NVARCHAR(600)    NULL,
        -- Titulo e texto do anuncio que a pessoa viu: ajuda a saber qual criativo funcionou
        Headline            NVARCHAR(600)    NULL,
        Corpo               NVARCHAR(1000)   NULL,

        DataPrimeiroContato DATETIME         NOT NULL CONSTRAINT DF_OrigemLead_Data DEFAULT (GETDATE()),
        ConversaoEnviada    BIT              NOT NULL CONSTRAINT DF_OrigemLead_Conversao DEFAULT (0),

        CONSTRAINT FK_OrigemLead_Empresa FOREIGN KEY (EmpresaId) REFERENCES dbo.Empresa (Id)
    );

    CREATE INDEX IX_OrigemLead_EmpresaId ON dbo.OrigemLead (EmpresaId);

    -- Uma origem por telefone dentro da empresa: a pessoa pode mandar varias mensagens, mas o
    -- clique que a trouxe e um so. Sem isto, cada mensagem repetiria o registro.
    CREATE UNIQUE INDEX UX_OrigemLead_Empresa_Telefone ON dbo.OrigemLead (EmpresaId, Telefone);
END
GO
