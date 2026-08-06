USE ContactSolutionDB;
GO

-- Suporte a midia (imagem/audio/video/documento) recebida e enviada no chat.
-- MidiaId guarda o media id da Meta (usado pra baixar/reenviar depois via Graph API);
-- TipoMidia guarda o tipo simplificado (image/audio/video/document) pro front decidir
-- como renderizar a bolha do chat.
IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[MensagemRecebida]') AND type = 'U')
BEGIN
    IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[MensagemRecebida]') AND name = 'MidiaId')
        ALTER TABLE MensagemRecebida ADD MidiaId NVARCHAR(100) NULL;

    IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[MensagemRecebida]') AND name = 'TipoMidia')
        ALTER TABLE MensagemRecebida ADD TipoMidia NVARCHAR(20) NULL;
END
GO

IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[HistoricoDisparo]') AND type = 'U')
BEGIN
    IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[HistoricoDisparo]') AND name = 'MidiaId')
        ALTER TABLE HistoricoDisparo ADD MidiaId NVARCHAR(100) NULL;

    IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[HistoricoDisparo]') AND name = 'TipoMidia')
        ALTER TABLE HistoricoDisparo ADD TipoMidia NVARCHAR(20) NULL;
END
GO
