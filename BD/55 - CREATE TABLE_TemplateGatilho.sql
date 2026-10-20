USE ContactSolutionDB;
GO

-- Momentos de disparo automatico de um template (tela de Templates): "ao cadastrar contato" e
-- "atraso de pagamento". Um gatilho de cada tipo por template. VariaveisJson segue o mesmo
-- formato do Agendamento (AgendamentoVariavelDto), resolvido pelo ResolvedorDeVariaveis.
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='TemplateGatilho' AND xtype='U')
BEGIN
    CREATE TABLE TemplateGatilho (
        Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
        EmpresaId UNIQUEIDENTIFIER NOT NULL,
        TemplateId UNIQUEIDENTIFIER NOT NULL,
        Tipo NVARCHAR(30) NOT NULL,
        -- So para ATRASO_PAGAMENTO: dias depois do vencimento da cobranca ainda nao paga.
        DiasAtraso INT NULL,
        VariaveisJson NVARCHAR(MAX) NULL,
        Ativo BIT NOT NULL DEFAULT (1),
        DataCriacao DATETIME NOT NULL DEFAULT (GETDATE()),
        DataAtualizacao DATETIME NULL,

        CONSTRAINT FK_TemplateGatilho_Empresa FOREIGN KEY (EmpresaId) REFERENCES Empresa(Id),
        -- Excluir o template leva os gatilhos junto: gatilho sem template nao tem o que enviar.
        CONSTRAINT FK_TemplateGatilho_Template FOREIGN KEY (TemplateId) REFERENCES Template(Id) ON DELETE CASCADE,
        CONSTRAINT UQ_TemplateGatilho_Template_Tipo UNIQUE (TemplateId, Tipo)
    );
END
GO

-- Uma linha por cobranca ja avisada por um gatilho de atraso: o job roda todo dia e sem isso
-- mandaria o mesmo aviso de novo a cada passada enquanto a cobranca seguir em aberto.
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='TemplateGatilhoEnvio' AND xtype='U')
BEGIN
    CREATE TABLE TemplateGatilhoEnvio (
        GatilhoId UNIQUEIDENTIFIER NOT NULL,
        CobrancaClienteId UNIQUEIDENTIFIER NOT NULL,
        DataEnvio DATETIME NOT NULL DEFAULT (GETDATE()),

        CONSTRAINT PK_TemplateGatilhoEnvio PRIMARY KEY (GatilhoId, CobrancaClienteId),
        CONSTRAINT FK_TemplateGatilhoEnvio_Gatilho FOREIGN KEY (GatilhoId) REFERENCES TemplateGatilho(Id) ON DELETE CASCADE
    );
END
GO
