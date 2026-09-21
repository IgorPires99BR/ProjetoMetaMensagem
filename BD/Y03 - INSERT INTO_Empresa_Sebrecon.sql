USE ContactSolutionDB;
GO

/* SCRIPT DE CARGA IDEMPOTENTE
   Cria a empresa "Sebrecon" (cliente que revende a plataforma pros proprios clientes de
   cobranca) reaproveitando WabaId/PhoneNumberId/MetaAccessToken da empresa "Contact
   Solution" ja ativa -- combinado com o Igor, os dois tenants dividem o mesmo numero
   conectado por enquanto. Demais dados (CNPJ/telefone/e-mail) sao ficticios.
   Cria tambem o usuario "Gilson", titular da Sebrecon (perfil admin).
*/

DECLARE @EmpresaOrigemId UNIQUEIDENTIFIER;
DECLARE @WabaId NVARCHAR(100);
DECLARE @PhoneNumberId NVARCHAR(100);
DECLARE @MetaAccessToken NVARCHAR(MAX);
DECLARE @AppIdMeta NVARCHAR(100);

SELECT TOP 1
    @EmpresaOrigemId = Id,
    @WabaId = WabaId,
    @PhoneNumberId = PhoneNumberId,
    @MetaAccessToken = MetaAccessToken,
    @AppIdMeta = AppIdMeta
FROM Empresa
WHERE Nome = 'Contact Solution';

IF @EmpresaOrigemId IS NULL
BEGIN
    PRINT 'Empresa "Contact Solution" nao encontrada -- rode o Y01 antes deste script.';
    RETURN;
END

DECLARE @EmpresaSebreconId UNIQUEIDENTIFIER = NEWID();
DECLARE @UsuarioGilsonId UNIQUEIDENTIFIER = NEWID();

-- 1. Inserir Empresa "Sebrecon"
IF NOT EXISTS (SELECT 1 FROM Empresa WHERE Nome = 'Sebrecon')
BEGIN
    INSERT INTO Empresa (
        Id, Nome, Email, Cnpj, Telefone, WabaId, MetaAccessToken, PhoneNumberId, AppIdMeta,
        StatusConta, PlanoId, DataCriacao
    )
    VALUES (
        @EmpresaSebreconId, 'Sebrecon', 'contato@sebrecon.com.br', '00000000000100',
        '1100000001', @WabaId, @MetaAccessToken, @PhoneNumberId, @AppIdMeta,
        'Ativo', 'Bronze', GETDATE()
    );

    PRINT 'Empresa "Sebrecon" criada com sucesso.';
END
ELSE
BEGIN
    SELECT @EmpresaSebreconId = Id FROM Empresa WHERE Nome = 'Sebrecon';
    PRINT 'Empresa "Sebrecon" ja existia. ID recuperado.';
END

-- 2. Inserir Usuario "Gilson" (titular/admin da Sebrecon)
-- SenhaHash abaixo e o hash BCrypt real (custo 11) de "Sebrecon@123" -- gerado com
-- BCrypt.Net-Next 4.0.3, o mesmo pacote usado em CriaUsuarioHandler. Diferente do seed
-- antigo de Igor/Jose em Y01 (que guarda a senha em texto puro e nao bate no
-- BCrypt.Verify do login), aqui o login funciona de fato.
IF NOT EXISTS (SELECT 1 FROM Usuario WHERE Email = 'gilson@sebrecon.com.br')
BEGIN
    INSERT INTO Usuario (Id, EmpresaId, Nome, Email, SenhaHash, IsAdmin, DataCriacao)
    VALUES (
        @UsuarioGilsonId, @EmpresaSebreconId, 'Gilson', 'gilson@sebrecon.com.br',
        '$2a$11$zil1UvoDQLqxuSM/rQtlWO2ll9OaB0EyL3JIE1VWwK7k.2kLjMyMu', 1, GETDATE()
    );

    PRINT 'Usuario "Gilson" criado com sucesso. Senha temporaria: Sebrecon@123 (trocar no primeiro login).';
END
ELSE
BEGIN
    PRINT 'Usuario "Gilson" ja existe.';
END
GO
