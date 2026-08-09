USE ContactSolutionDB;
GO

-- Traduz para portugues os poucos nomes de tabela/coluna que ainda estavam em ingles.
-- Identificadores tecnicos da API da Meta (WabaId, PhoneNumberId, AppIdMeta, SystemUserToken,
-- Token*) sao mantidos como estao de proposito -- ja espelham a nomenclatura oficial da API
-- da Meta e traduzi-los so atrapalharia quem for cruzar com a documentacao deles.
-- Os nomes de classe/propriedade em C# (Flow, FlowEtapa, ConversationState, WebhookConfig,
-- FlowId) continuam os mesmos por enquanto -- isso faz parte de uma etapa separada, ainda nao
-- decidida, de renomeacao de UseCases/pastas em ingles.

IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Flow]') AND type = 'U')
   AND NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Fluxo]') AND type = 'U')
    EXEC sp_rename 'Flow', 'Fluxo';
GO

IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[FlowEtapa]') AND type = 'U')
   AND NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[FluxoEtapa]') AND type = 'U')
    EXEC sp_rename 'FlowEtapa', 'FluxoEtapa';
GO

IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[ConversationState]') AND type = 'U')
   AND NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[EstadoConversa]') AND type = 'U')
    EXEC sp_rename 'ConversationState', 'EstadoConversa';
GO

IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[WebhookConfig]') AND type = 'U')
   AND NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[ConfiguracaoWebhook]') AND type = 'U')
    EXEC sp_rename 'WebhookConfig', 'ConfiguracaoWebhook';
GO

IF EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[Empresa]') AND name = 'StatusAccount')
    EXEC sp_rename 'Empresa.StatusAccount', 'StatusConta', 'COLUMN';
GO
