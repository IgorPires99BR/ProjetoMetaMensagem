-- Limpeza pontual em prod (2026-10-04). Sebrecon divide o WABA da Contact Solution e a
-- sincronizacao com a Meta copiava os templates de uma empresa para a outra (corrigido no
-- AtualizaTemplateMetaHandler). Template agora pertence a empresa que o usa/criou:
--  * aviso_de_vencimento_atualizado ("Sebrecon - Cobranca") fica so na Sebrecon; os 5 disparos
--    antigos da copia da Contact Solution passam a apontar para ele (EmpresaId do historico
--    continua Contact Solution, que foi quem disparou).
--  * copias na Sebrecon dos templates de teste da Contact Solution saem (sem vinculos).
-- Ids fixos de prod: nao rodar em outro ambiente. Sem USE (Azure SQL nao aceita).
SET XACT_ABORT ON;
BEGIN TRANSACTION;

UPDATE HistoricoDisparo
SET TemplateId = '80522853-E2B1-43D0-93FC-AA05CFA52A3B' -- copia da Sebrecon
WHERE TemplateId = '09338867-F563-41D8-973D-1BD3CDA06D46'; -- copia da Contact Solution

DELETE FROM Template
WHERE EmpresaId = '76440438-D7BA-4E0E-B92C-A5B291233FDC' -- Contact Solution
  AND Id = '09338867-F563-41D8-973D-1BD3CDA06D46';

DELETE FROM Template
WHERE EmpresaId = 'F68556A7-1EC1-4D4D-B6C2-7663DCF8F70E' -- Sebrecon
  AND Id IN (
    '18B5D842-05E0-44D6-B278-88897662B038', -- aviso_de_vencimento
    'DC89C30F-2758-4AEF-9A20-34AA143B26C5', -- hello_world
    'C624B0D1-1092-4E3D-BF1C-666FA7EBFEAD', -- pagamento_confirmado_atendente_liga
    '0CAFFD37-5DCF-45D8-950C-F1AD3DA36F85', -- teste_variavel_claude_4639
    'F768C68E-C0A0-4354-8FCB-BE4FFFF1B227'  -- teste1
  );

COMMIT TRANSACTION;
