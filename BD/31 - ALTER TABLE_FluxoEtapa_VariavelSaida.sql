USE ContactSolutionDB;
GO

-- Nome da variavel onde a resposta do cliente e guardada numa etapa "Capturar Input".
--
-- A tela de Flows sempre teve o campo "Variavel de saida" e o comando sempre recebeu o valor,
-- mas nao havia coluna: o handler descartava em silencio. O orquestrador tentava adivinhar a
-- variavel procurando {{algo}} DENTRO do texto da pergunta -- o que so funcionaria se o usuario
-- escrevesse "{{nome}}" na pergunta, e aí o cliente veria "{{nome}}" cru na mensagem.
--
-- Resultado pratico: nenhum flow conseguia capturar dado nenhum. Toda etapa de captura gravava
-- vazio e {{nome}} nunca era substituido nas mensagens seguintes.
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[FluxoEtapa]') AND name = 'VariavelSaida')
    ALTER TABLE dbo.FluxoEtapa ADD VariavelSaida NVARCHAR(60) NULL;
GO
