USE ContactSolutionDB;
GO

-- Exclusao logica do contato: quem ja recebeu disparo (ou tem conversa, cobranca, tag...) nao
-- pode sair do banco sem levar o historico junto, entao "excluir" passa a so inativar. Inativo
-- some das listas e dos disparos; reativar (ou cadastrar o numero de novo) dispara de novo o
-- gatilho de cadastro do TemplateGatilho.
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[Contato]') AND name = 'Ativo')
    ALTER TABLE Contato ADD Ativo BIT NOT NULL CONSTRAINT DF_Contato_Ativo DEFAULT (1);
GO

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[Contato]') AND name = 'DataInativacao')
    ALTER TABLE Contato ADD DataInativacao DATETIME NULL;
GO
