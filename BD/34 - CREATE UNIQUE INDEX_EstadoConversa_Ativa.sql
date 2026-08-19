-- Duas mensagens do mesmo cliente chegando quase juntas (webhooks concorrentes) faziam duas
-- requisicoes lerem "nenhuma conversa ativa" antes de qualquer uma commitar, e as duas criavam
-- uma EstadoConversa nova -- ate 3 linhas duplicadas pro mesmo contato foram vistas ao vivo em
-- 18/08/2026, quebrando a tela de Chats Ativos (dicionario por contato com chave repetida).
--
-- Este indice unico filtrado garante no banco que so existe UMA conversa nao finalizada por
-- contato; a segunda insercao concorrente falha com erro de constraint, e o codigo (ver
-- FlowOrchestratorService) trata isso reprocessando contra a conversa que venceu a corrida.
CREATE UNIQUE INDEX UX_EstadoConversa_Ativa ON EstadoConversa(EmpresaId, ContatoId) WHERE Finalizado = 0;
