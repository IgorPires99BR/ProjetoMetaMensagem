USE ContactSolutionDB;
GO

-- Assinatura do cliente na Cakto (plataforma de pagamento). A conta na Contact Solution passa a
-- nascer do pagamento: o webhook da Cakto cria Empresa + Usuario admin e grava a assinatura aqui,
-- que vira a fonte da verdade sobre "esta empresa pode usar a plataforma hoje?".
--
-- EventoIdCakto guarda o id do ultimo evento processado: a Cakto reenvia o mesmo evento ate 5
-- vezes quando nao recebe 2xx em 8s, e sem isso uma renovacao entraria em duplicidade.
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Assinatura')
BEGIN
    CREATE TABLE dbo.Assinatura (
        Id                    UNIQUEIDENTIFIER NOT NULL CONSTRAINT PK_Assinatura PRIMARY KEY,
        EmpresaId             UNIQUEIDENTIFIER NOT NULL,

        -- Identificadores do lado da Cakto
        AssinaturaIdCakto     NVARCHAR(120)    NULL,
        ClienteIdCakto        NVARCHAR(120)    NULL,
        OfertaIdCakto         NVARCHAR(120)    NULL,
        EmailComprador        NVARCHAR(200)    NULL,

        -- Situacao comercial
        Plano                 NVARCHAR(40)     NOT NULL,  -- STARTER | PRO | ENTERPRISE
        Status                NVARCHAR(40)     NOT NULL,  -- ATIVA | INADIMPLENTE | CANCELADA | REEMBOLSADA
        ValorCentavos         INT              NULL,
        DataInicio            DATETIME         NOT NULL CONSTRAINT DF_Assinatura_DataInicio DEFAULT (GETDATE()),
        DataProximaCobranca   DATETIME         NULL,
        DataCancelamento      DATETIME         NULL,

        -- Rastro do webhook
        EventoIdCakto         NVARCHAR(120)    NULL,
        UltimoEvento          NVARCHAR(80)     NULL,
        DataUltimoEvento      DATETIME         NULL,

        DataCriacao           DATETIME         NOT NULL CONSTRAINT DF_Assinatura_DataCriacao DEFAULT (GETDATE()),
        DataAtualizacao       DATETIME         NULL,

        CONSTRAINT FK_Assinatura_Empresa FOREIGN KEY (EmpresaId) REFERENCES dbo.Empresa (Id)
    );

    CREATE INDEX IX_Assinatura_EmpresaId ON dbo.Assinatura (EmpresaId);

    -- A busca mais quente do webhook e "ja existe assinatura com este id da Cakto?".
    -- Filtrado porque assinatura avulsa (venda unica, sem recorrencia) fica com o campo nulo.
    CREATE UNIQUE INDEX UX_Assinatura_AssinaturaIdCakto ON dbo.Assinatura (AssinaturaIdCakto)
        WHERE AssinaturaIdCakto IS NOT NULL;
END
GO

-- Eventos ja processados, para descartar reenvio da Cakto sem repetir efeito (criar conta duas
-- vezes, por exemplo). Tabela separada da Assinatura porque nem todo evento tem assinatura --
-- uma compra recusada, por exemplo, nao cria nada.
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'EventoCakto')
BEGIN
    CREATE TABLE dbo.EventoCakto (
        Id             UNIQUEIDENTIFIER NOT NULL CONSTRAINT PK_EventoCakto PRIMARY KEY,
        EventoIdCakto  NVARCHAR(120)    NOT NULL,
        Evento         NVARCHAR(80)     NOT NULL,
        EmpresaId      UNIQUEIDENTIFIER NULL,
        PayloadJson    NVARCHAR(MAX)    NULL,
        DataRecebido   DATETIME         NOT NULL CONSTRAINT DF_EventoCakto_Data DEFAULT (GETDATE())
    );

    -- O par (evento, id) e a chave de idempotencia: a mesma venda gera eventos diferentes
    -- (aprovada, assinatura criada) compartilhando o mesmo id.
    CREATE UNIQUE INDEX UX_EventoCakto_Evento_Id ON dbo.EventoCakto (EventoIdCakto, Evento);
END
GO

-- Quando o numero da Meta foi conectado: e daqui que contam os 7 dias de periodo de garantia,
-- em que o envio fica limitado a 1.000 mensagens/dia. Contar do cadastro nao servia -- o cliente
-- gastaria o teste esperando a Meta aprovar o numero dele.
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[Empresa]') AND name = 'DataConexaoNumero')
    ALTER TABLE dbo.Empresa ADD DataConexaoNumero DATETIME NULL;
GO
