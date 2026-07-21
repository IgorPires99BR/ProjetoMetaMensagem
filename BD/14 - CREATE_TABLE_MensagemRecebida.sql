CREATE TABLE MensagemRecebida (
    Id UNIQUEIDENTIFIER NOT NULL,
    EmpresaId UNIQUEIDENTIFIER NOT NULL,
    ContatoId UNIQUEIDENTIFIER NULL,
    TelefoneRemetente VARCHAR(20) NOT NULL,
    Conteudo NVARCHAR(MAX) NOT NULL, -- NVARCHAR(MAX) para suportar textos longos e caracteres especiais/emojis
    Tipo VARCHAR(20) NOT NULL DEFAULT 'recebida', -- 'recebida' ou 'enviada'
    DataRecebimento DATETIME2 NOT NULL DEFAULT GETDATE(),
    Lida BIT NOT NULL DEFAULT 0,
    FlowId UNIQUEIDENTIFIER NULL,

    -- Chave Primária
    CONSTRAINT PK_MensagemRecebida PRIMARY KEY CLUSTERED (Id),

    -- Chaves Estrangeiras (Ajuste o nome das tabelas pai se forem diferentes)
    CONSTRAINT FK_MensagemRecebida_Companies FOREIGN KEY (EmpresaId) 
        REFERENCES Empresa (Id) ON DELETE CASCADE,
        
    CONSTRAINT FK_MensagemRecebida_Contatos FOREIGN KEY (ContatoId) 
        REFERENCES Contato (Id) ON DELETE NO ACTION
);


-- Verifica se a FK existe antes de tentar remover
IF EXISTS (
    SELECT 1 
    FROM sys.foreign_keys 
    WHERE name = 'FK_MensagemRecebida_Contatos' 
      AND parent_object_id = OBJECT_ID('MensagemRecebida')
)
BEGIN
    ALTER TABLE MensagemRecebida
    DROP CONSTRAINT FK_MensagemRecebida_Contatos;
    
    PRINT 'Chave estrangeira FK_MensagemRecebida_Contatos removida com sucesso.';
END
ELSE
BEGIN
    PRINT 'A chave estrangeira FK_MensagemRecebida_Contatos não existe ou já foi removida.';
END;