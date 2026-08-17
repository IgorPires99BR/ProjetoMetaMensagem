-- Dedupe de webhook reentregue pela Meta: sem isto, a mesma mensagem do cliente processava
-- duas vezes (duas conversas criadas, resposta do bot em dobro). Visto ao vivo com um lead
-- real do anuncio em 17/08/2026 (contato "Biguilo Luiz").
ALTER TABLE MensagemRecebida ADD WamidRecebido NVARCHAR(100) NULL;

-- Indice filtrado (so cobre linhas com wamid preenchido) -- rede de seguranca contra a
-- checagem de aplicacao perder uma corrida em caso de dois webhooks concorrentes de verdade.
CREATE UNIQUE INDEX UX_MensagemRecebida_Wamid ON MensagemRecebida(WamidRecebido) WHERE WamidRecebido IS NOT NULL;
