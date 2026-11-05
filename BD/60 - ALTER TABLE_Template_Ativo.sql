USE ContactSolutionDB;
GO

-- Inativacao do template: some da tela de Templates, de Disparos e de Agendamentos, para de sair
-- pelos momentos de disparo e continua no banco e na Meta. Reativar traz tudo de volta sem nova
-- analise (excluir da Meta e irreversivel e bloqueia o nome por 30 dias).
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[Template]') AND name = 'Ativo')
    ALTER TABLE Template ADD Ativo BIT NOT NULL CONSTRAINT DF_Template_Ativo DEFAULT (1);
GO

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[Template]') AND name = 'DataInativacao')
    ALTER TABLE Template ADD DataInativacao DATETIME NULL;
GO
