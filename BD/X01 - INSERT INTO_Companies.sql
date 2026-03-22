-- 1. Verifica se o registro com este e-mail já existe
IF NOT EXISTS (SELECT 1 FROM companies WHERE email = 'admin@master.com')
BEGIN
    -- 2. Insere apenas se não existir
    INSERT INTO companies (
        id, 
        name, 
        email, 
        password, 
        phone, 
        bot_whatsapp, 
        created_at
    )
    VALUES (
        'MASTER',                      -- id
        'Administrador Geral',         -- name
        'admin@master.com',            -- email
        'suasenha',                    -- password (ideal usar hash em produção)
        '+5511956946084171393',        -- phone (exemplo)
        NULL,                          -- bot_whatsapp
        SYSDATETIMEOFFSET()            -- created_at
    );
    
    PRINT 'Registro inserido com sucesso.';
END
ELSE
BEGIN
    PRINT 'O registro já existe na tabela companies.';
END
GO