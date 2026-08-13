-- O motivo da falha de entrega vindo da Meta so trafegava por SignalR (evento
-- AtualizaStatusEntrega). Sem coluna pra guardar, um reload da conversa perdia o
-- motivo e so sobrava o status "failed" sem explicacao.
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('dbo.HistoricoDisparo') AND name = 'MotivoFalha')
BEGIN
    ALTER TABLE HistoricoDisparo ADD MotivoFalha NVARCHAR(500) NULL;
END
