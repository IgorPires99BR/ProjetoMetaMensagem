-- Tabela para armazenar mensagens recebidas e enviadas no Shared Inbox
-- Suporta o novo módulo de Chat e o processamento de webhook

IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'MensagemRecebida')
BEGIN
    CREATE TABLE MensagemRecebida (
        Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
        EmpresaId UNIQUEIDENTIFIER NOT NULL,
        ContatoId UNIQUEIDENTIFIER NULL,
        TelefoneRemetente VARCHAR(20) NOT NULL,
        Conteudo VARCHAR(MAX) NOT NULL,
        Tipo VARCHAR(20) NOT NULL DEFAULT 'recebida', -- 'recebida' ou 'enviada'
        DataRecebimento DATETIME NOT NULL DEFAULT GETDATE(),
        Lida BIT NOT NULL DEFAULT 0,
        FlowId UNIQUEIDENTIFIER NULL,
        CONSTRAINT FK_MensagemRecebida_Empresa FOREIGN KEY (EmpresaId) REFERENCES Empresa(Id)
    );

    -- Indices para consultas do Shared Inbox
    CREATE INDEX IX_MensagemRecebida_EmpresaId ON MensagemRecebida(EmpresaId, DataRecebimento DESC);
    CREATE INDEX IX_MensagemRecebida_Contato ON MensagemRecebida(EmpresaId, ContatoId, DataRecebimento);
    CREATE INDEX IX_MensagemRecebida_NaoLidas ON MensagemRecebida(EmpresaId, ContatoId, Lida) WHERE Lida = 0;

    PRINT 'Tabela MensagemRecebida criada com sucesso.';
END
ELSE
BEGIN
    PRINT 'Tabela MensagemRecebida ja existe.';
END
GO
