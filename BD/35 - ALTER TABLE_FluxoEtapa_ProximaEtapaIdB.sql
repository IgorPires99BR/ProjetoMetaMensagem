-- Ramificacao de duas saidas: quando a etapa tem botoes e a resposta do cliente casa com o
-- Botao2, o flow salta pra ProximaEtapaIdB em vez de seguir por ProximaEtapaId. Necessario pro
-- flow "Escolha sua porta", onde quem aperta "Quanto custa?" pula a explicacao e vai direto aos
-- planos. NULL mantem o comportamento linear de sempre.
ALTER TABLE FluxoEtapa ADD ProximaEtapaIdB UNIQUEIDENTIFIER NULL;
