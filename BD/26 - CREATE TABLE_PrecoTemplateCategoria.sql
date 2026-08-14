USE ContactSolutionDB;
GO

-- Preco por categoria de template (Marketing/Utility/Authentication), usado pra estimar o
-- gasto com disparos no relatorio financeiro. A Meta cobra por conversa iniciada com template,
-- por categoria e por pais/tier de qualidade da conta -- nao ha API que devolva esse valor pra
-- gente consultar, entao o preco fica configuravel aqui (editado pela propria tela do relatorio)
-- em vez de um numero fixo no codigo que ficaria errado assim que a Meta reajustar.
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[PrecoTemplateCategoria]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[PrecoTemplateCategoria](
        [Categoria] [nvarchar](50) NOT NULL,
        [PrecoUnitario] [decimal](10,4) NOT NULL DEFAULT (0),
        [Moeda] [nvarchar](3) NOT NULL DEFAULT ('BRL'),
        [DataAtualizacao] [datetime] NOT NULL DEFAULT (GETDATE()),

        CONSTRAINT [PK_PrecoTemplateCategoria] PRIMARY KEY CLUSTERED ([Categoria] ASC)
    );
END
GO

IF NOT EXISTS (SELECT * FROM [dbo].[PrecoTemplateCategoria] WHERE [Categoria] = 'MARKETING')
    INSERT INTO [dbo].[PrecoTemplateCategoria] ([Categoria], [PrecoUnitario]) VALUES ('MARKETING', 0);
GO

IF NOT EXISTS (SELECT * FROM [dbo].[PrecoTemplateCategoria] WHERE [Categoria] = 'UTILITY')
    INSERT INTO [dbo].[PrecoTemplateCategoria] ([Categoria], [PrecoUnitario]) VALUES ('UTILITY', 0);
GO

IF NOT EXISTS (SELECT * FROM [dbo].[PrecoTemplateCategoria] WHERE [Categoria] = 'AUTHENTICATION')
    INSERT INTO [dbo].[PrecoTemplateCategoria] ([Categoria], [PrecoUnitario]) VALUES ('AUTHENTICATION', 0);
GO
