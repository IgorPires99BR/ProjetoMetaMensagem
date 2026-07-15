USE ContactSolutionDB -- Substitua pelo nome do seu banco
GO

-- 1. Adiciona os novos campos na tabela Template
ALTER TABLE [dbo].[Template] 
ADD 
    [MetaTemplateId] [nvarchar](100) NULL, -- Inicialmente NULL para não quebrar dados legados se houver
    [ComponentesJson] [nvarchar](max) NULL;
GO

-- 2. OPCIONAL: Se você já tiver dados na tabela e quiser torná-lo NOT NULL após atualizar os antigos, 
-- você pode rodar um update falso usando o Id antigo ou o Nome, e depois alterar para NOT NULL:
/*
UPDATE [dbo].[Template] 
SET [MetaTemplateId] = 'LEGADO_' + CAST([Id] as nvarchar(36)) 
WHERE [MetaTemplateId] IS NULL;

ALTER TABLE [dbo].[Template] 
ALTER COLUMN [MetaTemplateId] [nvarchar](100) NOT NULL;
GO
*/

-- 3. Criar um índice não-clusterizado (Non-Clustered Index) para o MetaTemplateId
-- Como seu Handler faz um .FirstOrDefault(x => x.MetaTemplateId == templateApi.Id) dentro de um loop,
-- esse índice é CRÍTICO para a performance do banco de dados não despencar conforme o volume crescer.
CREATE NONCLUSTERED INDEX [IX_Template_MetaTemplateId_EmpresaId] 
ON [dbo].[Template] ([MetaTemplateId], [EmpresaId])
GO