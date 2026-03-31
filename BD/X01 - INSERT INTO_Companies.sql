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

IF NOT EXISTS (SELECT 1 FROM companies WHERE email = 'vtrzmartil@gmail.com')
BEGIN
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
        'JOSE_SOCIO',                  -- id único
        'José',                        -- name
        'vtrzmartil@gmail.com',        -- email
        'viz1x2c3v4',                  -- password
        NULL,                          -- phone
        NULL,                          -- bot_whatsapp
        SYSDATETIMEOFFSET()            -- created_at
    );
    PRINT 'Registro do José inserido com sucesso.';
END
ELSE
BEGIN
    PRINT 'O e-mail vtrzmartil@gmail.com já existe.';
END

-- 2. REGISTRO PARA IGOR
IF NOT EXISTS (SELECT 1 FROM companies WHERE email = 'igorpires97@gmail.com')
BEGIN
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
        'IGOR_SOCIO',                  -- id único
        'Igor',                        -- name
        'igorpires97@gmail.com',       -- email
        'igor@123',                    -- password
        NULL,                          -- phone
        NULL,                          -- bot_whatsapp
        SYSDATETIMEOFFSET()            -- created_at
    );
    PRINT 'Registro do Igor inserido com sucesso.';
END
ELSE
BEGIN
    PRINT 'O e-mail igorpires97@gmail.com já existe.';
END
GO


GO