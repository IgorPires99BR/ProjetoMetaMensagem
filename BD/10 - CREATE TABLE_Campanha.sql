IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='Campanha' AND xtype='U')
BEGIN
    CREATE TABLE Campanha (
        Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
        EmpresaId UNIQUEIDENTIFIER NOT NULL,
        Nome NVARCHAR(200) NOT NULL,
        TemplateId UNIQUEIDENTIFIER NULL,
        ConteudoLivre NVARCHAR(MAX) NULL,
        DataAgendamento DATETIME NOT NULL,
        Status NVARCHAR(50) DEFAULT 'AGENDADA',
        TotalContatos INT DEFAULT 0,
        Processados INT DEFAULT 0,
        DataCriacao DATETIME DEFAULT GETDATE(),
        CONSTRAINT FK_Campanha_Empresa FOREIGN KEY (EmpresaId) REFERENCES Empresa(Id)
    );
END;

IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='CampanhaContato' AND xtype='U')
BEGIN
    CREATE TABLE CampanhaContato (
        Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
        CampanhaId UNIQUEIDENTIFIER NOT NULL,
        ContatoId UNIQUEIDENTIFIER NOT NULL,
        Processado BIT DEFAULT 0,
        Sucesso BIT NULL,
        MensagemErro NVARCHAR(MAX) NULL,
        CONSTRAINT FK_CampanhaContato_Campanha FOREIGN KEY (CampanhaId) REFERENCES Campanha(Id),
        CONSTRAINT FK_CampanhaContato_Contato FOREIGN KEY (ContatoId) REFERENCES Contato(Id)
    );
END;
