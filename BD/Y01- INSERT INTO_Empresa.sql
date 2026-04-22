USE ContactSolutionDB;
GO

/* SCRIPT DE CARGA INICIAL IDEMPOTENTE
   Este script utiliza variáveis para garantir que os mesmos GUIDs sejam usados 
   entre os relacionamentos de Empresa e Usuário durante a execução.
*/

DECLARE @EmpresaId UNIQUEIDENTIFIER = NEWID();
DECLARE @UsuarioIgorId UNIQUEIDENTIFIER = NEWID();
DECLARE @UsuarioJoseId UNIQUEIDENTIFIER = NEWID();

-- 1. Inserir Empresa "Contact Solution"
IF NOT EXISTS (SELECT 1 FROM Empresa WHERE Nome = 'Contact Solution')
BEGIN
    INSERT INTO Empresa (Id, Nome, Email, Telefone, DataCriacao)
    VALUES (@EmpresaId, 'Contact Solution', 'contato@contactsolution.com.br', '1100000000', GETDATE());
    
    PRINT 'Empresa "Contact Solution" criada com sucesso.';
END
ELSE
BEGIN
    SELECT @EmpresaId = Id FROM Empresa WHERE Nome = 'Contact Solution';
    PRINT 'Empresa "Contact Solution" já existia. ID recuperado.';
END

-- 2. Inserir Usuário Igor
IF NOT EXISTS (SELECT 1 FROM Usuario WHERE Email = 'igorpires97@gmail.com')
BEGIN
    INSERT INTO Usuario (Id, EmpresaId, Nome, Email, SenhaHash, IsAdmin, DataCriacao)
    VALUES (@UsuarioIgorId, @EmpresaId, 'Igor', 'igorpires97@gmail.com', '123456', 1, GETDATE());
    
    PRINT 'Usuário "Igor" criado com sucesso.';
END
ELSE
BEGIN
    PRINT 'Usuário "Igor" já existe.';
END

-- 3. Inserir Usuário Jose Victor
IF NOT EXISTS (SELECT 1 FROM Usuario WHERE Email = 'vtrz@gmail.com')
BEGIN
    INSERT INTO Usuario (Id, EmpresaId, Nome, Email, SenhaHash, IsAdmin, DataCriacao)
    VALUES (@UsuarioJoseId, @EmpresaId, 'Jose Victor', 'vtrz@gmail.com', '123456', 1, GETDATE());
    
    PRINT 'Usuário "Jose Victor" criado com sucesso.';
END
ELSE
BEGIN
    PRINT 'Usuário "Jose Victor" já existe.';
END
GO