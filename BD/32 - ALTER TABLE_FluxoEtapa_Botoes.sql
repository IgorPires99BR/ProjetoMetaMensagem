-- Suporte a etapas de "Capturar Input" enviadas como botoes de resposta rapida (ate 2 opcoes),
-- em vez de texto simples. Usado no flow de qualificacao para perguntar Starter x Pro.
ALTER TABLE FluxoEtapa ADD Botao1 NVARCHAR(20) NULL;
ALTER TABLE FluxoEtapa ADD Botao2 NVARCHAR(20) NULL;
