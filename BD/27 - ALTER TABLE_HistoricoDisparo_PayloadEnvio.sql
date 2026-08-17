USE ContactSolutionDB;
GO

-- Ate aqui a coluna Conteudo acumulava dois papeis conflitantes: o texto que o cliente
-- recebeu e o JSON enviado a Meta (auditoria). Como o JSON ganhava a disputa nos disparos por
-- template, o chat e o relatorio mostravam {"ParametrosBody":[],"PayloadEnvio":... no lugar da
-- mensagem. Agora Conteudo guarda o texto legivel e a auditoria vive aqui.
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[HistoricoDisparo]') AND name = 'PayloadEnvio')
    ALTER TABLE dbo.HistoricoDisparo ADD PayloadEnvio NVARCHAR(MAX) NULL;
GO
