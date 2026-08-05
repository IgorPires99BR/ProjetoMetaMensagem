USE ContactSolutionDB;
GO

-- Adiciona coluna StatusEntrega para suportar o status de leitura (check cinza/duplo cinza/duplo azul)
-- vindo do webhook da Meta (value.statuses[].status: sent/delivered/read/failed).
IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[HistoricoDisparo]') AND type in (N'U'))
BEGIN
    IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[HistoricoDisparo]') AND name = 'StatusEntrega')
        ALTER TABLE HistoricoDisparo ADD StatusEntrega NVARCHAR(50) NULL;
END
GO
