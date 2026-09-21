USE ContactSolutionDB;
GO

BEGIN TRANSACTION;

-- Origem: de onde saiu o disparo -- "Disparador de Mensagem", "Agendador Automático",
-- "Flow Automático" ou "Chat Manual" (ver OrigemDisparo no dominio). Nullable porque
-- historico gravado antes desta coluna nao tem como saber a origem retroativamente.
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[HistoricoDisparo]') AND name = 'Origem')
BEGIN
    ALTER TABLE HistoricoDisparo ADD Origem NVARCHAR(50) NULL;
END

COMMIT TRANSACTION;
GO
