USE ContactSolutionDB;
GO

-- Ate aqui o FlowOrchestratorService rodava o flow em toda mensagem recebida, sem nenhuma forma
-- de saber se um vendedor ja assumiu a conversa manualmente pelo chat. AssumidoPorUsuarioId NULL
-- = flow tocando normalmente; preenchido = conversa pausada, assumida por aquele vendedor (o
-- registro de EtapaAtualId/Variaveis fica intacto pra poder devolver ao flow depois).
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[EstadoConversa]') AND name = 'AssumidoPorUsuarioId')
    ALTER TABLE dbo.EstadoConversa ADD AssumidoPorUsuarioId UNIQUEIDENTIFIER NULL;
GO

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[EstadoConversa]') AND name = 'DataAssumido')
    ALTER TABLE dbo.EstadoConversa ADD DataAssumido DATETIME NULL;
GO

IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_EstadoConversa_Usuario]') AND parent_object_id = OBJECT_ID(N'[dbo].[EstadoConversa]'))
BEGIN
    ALTER TABLE dbo.EstadoConversa
    ADD CONSTRAINT FK_EstadoConversa_Usuario FOREIGN KEY (AssumidoPorUsuarioId)
    REFERENCES dbo.Usuario (Id);
END
GO
