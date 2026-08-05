USE ContactSolutionDB;
GO

-- Cria a tabela TemplateConexao, usada pela visualizacao de mapa mental 3D dos templates,
-- para guardar as ligacoes (arestas) desenhadas manualmente entre templates de uma mesma empresa.
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[TemplateConexao]') AND type in (N'U'))
BEGIN
    CREATE TABLE TemplateConexao (
        Id UNIQUEIDENTIFIER NOT NULL DEFAULT NEWID() PRIMARY KEY,
        EmpresaId UNIQUEIDENTIFIER NOT NULL,
        TemplateOrigemId UNIQUEIDENTIFIER NOT NULL,
        TemplateDestinoId UNIQUEIDENTIFIER NOT NULL,
        BotaoTexto NVARCHAR(100) NULL,
        DataCriacao DATETIME NOT NULL DEFAULT GETDATE()
    );
END
GO
