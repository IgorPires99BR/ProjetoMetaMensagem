USE ContactSolutionDB;
GO

/* SCRIPT DE CARGA IDEMPOTENTE
   Cria o Perfil de acesso padrao da Sebrecon (todas as telas exceto Empresas e Flows -- a
   Sebrecon usa a plataforma pra cobranca via disparo/agendamento, nao monta chatbot nem
   administra outras empresas) e atribui esse perfil ao usuario Gilson.
*/

DECLARE @EmpresaSebreconId UNIQUEIDENTIFIER;
SELECT @EmpresaSebreconId = Id FROM Empresa WHERE Nome = 'Sebrecon';

IF @EmpresaSebreconId IS NULL
BEGIN
    PRINT 'Empresa "Sebrecon" nao encontrada -- rode o Y03 antes deste script.';
    RETURN;
END

DECLARE @PerfilId UNIQUEIDENTIFIER;
SELECT @PerfilId = Id FROM Perfil WHERE EmpresaId = @EmpresaSebreconId AND Nome = 'Padrão Sebrecon';

IF @PerfilId IS NULL
BEGIN
    SET @PerfilId = NEWID();

    INSERT INTO Perfil (Id, EmpresaId, Nome, DataCriacao)
    VALUES (@PerfilId, @EmpresaSebreconId, 'Padrão Sebrecon', GETDATE());

    -- Todas as telas do sistema, exceto 'empresas' (gestao de outros tenants -- so a conta de
    -- plataforma usa) e 'flows' (chatbot automatizado -- a Sebrecon dispara cobranca manual/
    -- agendada, nao monta fluxo de conversa).
    INSERT INTO PerfilTela (PerfilId, TelaChave)
    VALUES
        (@PerfilId, 'dashboard'),
        (@PerfilId, 'chats'),
        (@PerfilId, 'disparador'),
        (@PerfilId, 'agendamentos'),
        (@PerfilId, 'relatorio'),
        (@PerfilId, 'metricas'),
        (@PerfilId, 'contatos'),
        (@PerfilId, 'numeros'),
        (@PerfilId, 'usuarios'),
        (@PerfilId, 'cobrancas'),
        (@PerfilId, 'templates');

    PRINT 'Perfil "Padrão Sebrecon" criado com sucesso.';
END
ELSE
BEGIN
    PRINT 'Perfil "Padrão Sebrecon" ja existia. ID recuperado.';
END

-- Tela Parametros (BD/46): fora do INSERT acima de proposito -- assim tambem chega no perfil de
-- quem ja tinha rodado este script antes da tela existir.
IF NOT EXISTS (SELECT 1 FROM PerfilTela WHERE PerfilId = @PerfilId AND TelaChave = 'parametros')
    INSERT INTO PerfilTela (PerfilId, TelaChave) VALUES (@PerfilId, 'parametros');

UPDATE Usuario
SET PerfilId = @PerfilId
WHERE EmpresaId = @EmpresaSebreconId AND Email = 'gilson@sebrecon.com.br' AND PerfilId IS NULL;

PRINT 'Perfil atribuido ao usuario Gilson (se ainda nao tivesse um).';
GO
