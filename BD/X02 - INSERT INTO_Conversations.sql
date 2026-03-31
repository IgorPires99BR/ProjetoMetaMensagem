USE ContactSolutionDB
INSERT INTO Conversations (
    company_id, 
    phone, 
    status_funil, 
    status, 
    step, 
    nome, 
    email, 
    updated_at
) 
VALUES 
-- Lead em estágio inicial (Fase 1) controlado pelo Bot
(
    'MASTER', 
    '5511999998888', 
    '1', 
    'bot', 
    'boas-vindas', 
    'João Silva', 
    'joao.silva@email.com', 
    CURRENT_TIMESTAMP
),
-- Lead avançado para Negociação (Fase 2)
(
    'MASTER', 
    '5511977776666', 
    '2', 
    'negociacao', 
    'proposta-enviada', 
    'Maria Oliveira', 
    'maria.oliveira@empresa.com', 
    CURRENT_TIMESTAMP
),
-- Lead que já Concluiu a venda (Fase 4)
(
    'MASTER', 
    '5521955554444', 
    '4', 
    'concluida', 
    'pos-venda', 
    'Carlos Eduardo', 
    'cadu@email.com', 
    CURRENT_TIMESTAMP
),
-- Lead que foi Perdido (Fase 3)
(
    'MASTER', 
    '5531933332222', 
    '3', 
    'perdida', 
    'follow-up-recusado', 
    'Ana Beatriz', 
    'ana.bea@provedor.net', 
    CURRENT_TIMESTAMP
);