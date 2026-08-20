USE ContactSolutionDB;
GO

BEGIN TRANSACTION;

-- TentativasNaEtapa: quantas vezes seguidas o cliente respondeu algo que a etapa nao esperava
-- (numa pergunta de botao, algo que nao casa com nenhum botao). Na segunda, o bot para de
-- insistir e entrega a conversa pra uma pessoa -- reperguntar a mesma coisa uma terceira vez
-- so irrita e perde o lead.
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[EstadoConversa]') AND name = 'TentativasNaEtapa')
BEGIN
    ALTER TABLE EstadoConversa ADD TentativasNaEtapa INT NOT NULL DEFAULT 0;
END

-- AguardandoAtendente: marca a conversa que o bot desistiu de conduzir. A tela de Chats usa
-- isto pra destacar quem precisa de gente, em vez de o atendente ter que garimpar no meio de
-- todas as conversas.
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[EstadoConversa]') AND name = 'AguardandoAtendente')
BEGIN
    ALTER TABLE EstadoConversa ADD AguardandoAtendente BIT NOT NULL DEFAULT 0;
END

COMMIT TRANSACTION;
GO
