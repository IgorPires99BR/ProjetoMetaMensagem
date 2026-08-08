-- ============================================================================
-- 20 - Corrige o indice unico de WamidMeta e cria a coluna do gatilho do Flow
-- Idempotente: pode rodar mais de uma vez sem erro.
-- ============================================================================

-- Indice FILTRADO exige estas duas opcoes ligadas. O sqlcmd nao liga QUOTED_IDENTIFIER
-- por padrao, e sem isso o CREATE INDEX abaixo falha com a mensagem 1934.
SET QUOTED_IDENTIFIER ON;
SET ANSI_NULLS ON;
GO

-- ----------------------------------------------------------------------------
-- 1) HistoricoDisparo.WamidMeta
--
-- O indice era UNIQUE sem filtro. Quando a Meta recusa um envio, nao ha wamid
-- e o registro era gravado com string vazia; a partir do SEGUNDO envio falho o
-- INSERT estourava com violacao de chave duplicada. Efeito colateral perverso:
-- como no maximo uma falha por banco conseguia ser gravada, a Taxa de Entrega
-- do dashboard nunca conseguia cair.
--
-- A unicidade continua valendo para wamid de verdade (e o que liga o evento de
-- status vindo do webhook ao disparo), mas passa a ignorar nulo/vazio.
-- ----------------------------------------------------------------------------
IF EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_HistoricoDisparo_WamidMeta' AND object_id = OBJECT_ID('HistoricoDisparo'))
BEGIN
    DROP INDEX IX_HistoricoDisparo_WamidMeta ON HistoricoDisparo;
END
GO

-- A coluna era NOT NULL, o que obrigava a gravar string vazia quando a Meta nao
-- devolvia id de mensagem -- justamente o valor que colidia no indice unico.
-- Precisa aceitar NULL para o indice filtrado abaixo fazer sentido.
IF EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('HistoricoDisparo') AND name = 'WamidMeta' AND is_nullable = 0)
BEGIN
    ALTER TABLE HistoricoDisparo ALTER COLUMN WamidMeta nvarchar(255) NULL;
END
GO

-- Normaliza o que ja esta gravado: vazio vira NULL, para caber no indice filtrado.
UPDATE HistoricoDisparo SET WamidMeta = NULL WHERE WamidMeta = '';
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_HistoricoDisparo_WamidMeta' AND object_id = OBJECT_ID('HistoricoDisparo'))
BEGIN
    CREATE UNIQUE INDEX IX_HistoricoDisparo_WamidMeta
        ON HistoricoDisparo (WamidMeta)
        WHERE WamidMeta IS NOT NULL;
END
GO

-- ----------------------------------------------------------------------------
-- 2) Flow.GatilhoInicial
--
-- A tela de Flows sempre enviou e leu a palavra-chave que dispara o fluxo, e a
-- entidade Flow ja tinha a propriedade GatilhoInicial, mas a coluna nunca
-- existiu: o valor era descartado no INSERT e voltava vazio na leitura. Sem ela
-- nenhum flow pode ser acionado, e a validacao do front ficava travada exigindo
-- um campo que ela mesma nao conseguia manter preenchido.
--
-- O nome da coluna acompanha a propriedade da entidade porque os repositorios
-- montam o SQL com nameof(...).
-- ----------------------------------------------------------------------------
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('Flow') AND name = 'GatilhoInicial')
BEGIN
    ALTER TABLE Flow ADD GatilhoInicial nvarchar(200) NULL;
END
GO

-- Remove a coluna criada com o nome errado numa primeira versao deste script.
IF EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('Flow') AND name = 'GatilhoPalavraChave')
BEGIN
    ALTER TABLE Flow DROP COLUMN GatilhoPalavraChave;
END
GO

-- ----------------------------------------------------------------------------
-- 3) Limpeza: indices criados por teste de carga, nao fazem parte do schema.
-- ----------------------------------------------------------------------------
IF EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'ZZ_ESCALA_IX_HistDisparo_Empresa_Data' AND object_id = OBJECT_ID('HistoricoDisparo'))
    DROP INDEX ZZ_ESCALA_IX_HistDisparo_Empresa_Data ON HistoricoDisparo;
GO

IF EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'ZZ_ESCALA_IX_HistDisparo_Empresa_Contato_Data' AND object_id = OBJECT_ID('HistoricoDisparo'))
    DROP INDEX ZZ_ESCALA_IX_HistDisparo_Empresa_Contato_Data ON HistoricoDisparo;
GO
