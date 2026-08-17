USE ContactSolutionDB;
GO

-- O modulo de CRM (Pipeline/Leads) foi escrito por inteiro -- entidades, repositorio, casos de
-- uso e endpoints -- mas nunca teve migration: nem o banco local nem o de producao tinham estas
-- tres tabelas, e /api/pipeline/listar e /api/leads respondiam 500 desde sempre.
-- Os nomes e tipos aqui seguem exatamente o que o PipelineRepository consulta.

IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Pipeline')
BEGIN
    CREATE TABLE dbo.Pipeline (
        Id           UNIQUEIDENTIFIER NOT NULL CONSTRAINT PK_Pipeline PRIMARY KEY,
        EmpresaId    UNIQUEIDENTIFIER NOT NULL,
        Nome         NVARCHAR(150)    NOT NULL,
        DataCriacao  DATETIME         NOT NULL CONSTRAINT DF_Pipeline_DataCriacao DEFAULT (GETDATE()),
        CONSTRAINT FK_Pipeline_Empresa FOREIGN KEY (EmpresaId) REFERENCES dbo.Empresa (Id)
    );

    -- Toda listagem do CRM parte da empresa (escopo multi-tenant), nunca do Id solto.
    CREATE INDEX IX_Pipeline_EmpresaId ON dbo.Pipeline (EmpresaId);
END
GO

IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'PipelineEtapa')
BEGIN
    CREATE TABLE dbo.PipelineEtapa (
        Id                  UNIQUEIDENTIFIER NOT NULL CONSTRAINT PK_PipelineEtapa PRIMARY KEY,
        PipelineId          UNIQUEIDENTIFIER NOT NULL,
        Nome                NVARCHAR(150)    NOT NULL,
        Ordem               INT              NOT NULL CONSTRAINT DF_PipelineEtapa_Ordem DEFAULT (0),
        Cor                 NVARCHAR(20)     NULL,
        DispararAoEntrar    BIT              NOT NULL CONSTRAINT DF_PipelineEtapa_Disparar DEFAULT (0),
        -- Template opcional disparado quando o lead entra na etapa; sem FK com cascade de
        -- proposito, pra excluir um template nao apagar a etapa junto.
        TemplateIdAoEntrar  UNIQUEIDENTIFIER NULL,
        DataCriacao         DATETIME         NOT NULL CONSTRAINT DF_PipelineEtapa_DataCriacao DEFAULT (GETDATE()),
        CONSTRAINT FK_PipelineEtapa_Pipeline FOREIGN KEY (PipelineId) REFERENCES dbo.Pipeline (Id)
    );

    CREATE INDEX IX_PipelineEtapa_PipelineId ON dbo.PipelineEtapa (PipelineId, Ordem);
END
GO

IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'LeadPipeline')
BEGIN
    CREATE TABLE dbo.LeadPipeline (
        Id                   UNIQUEIDENTIFIER NOT NULL CONSTRAINT PK_LeadPipeline PRIMARY KEY,
        EmpresaId            UNIQUEIDENTIFIER NOT NULL,
        ContatoId            UNIQUEIDENTIFIER NOT NULL,
        PipelineEtapaId      UNIQUEIDENTIFIER NOT NULL,
        Valor                DECIMAL(18, 2)   NULL,
        Observacao           NVARCHAR(1000)   NULL,
        DataEntrada          DATETIME         NOT NULL CONSTRAINT DF_LeadPipeline_DataEntrada DEFAULT (GETDATE()),
        DataUltimaAlteracao  DATETIME         NOT NULL CONSTRAINT DF_LeadPipeline_DataAlteracao DEFAULT (GETDATE()),
        DataCriacao          DATETIME         NOT NULL CONSTRAINT DF_LeadPipeline_DataCriacao DEFAULT (GETDATE()),
        CONSTRAINT FK_LeadPipeline_Empresa FOREIGN KEY (EmpresaId) REFERENCES dbo.Empresa (Id),
        CONSTRAINT FK_LeadPipeline_Contato FOREIGN KEY (ContatoId) REFERENCES dbo.Contato (Id),
        CONSTRAINT FK_LeadPipeline_PipelineEtapa FOREIGN KEY (PipelineEtapaId) REFERENCES dbo.PipelineEtapa (Id)
    );

    CREATE INDEX IX_LeadPipeline_EmpresaId ON dbo.LeadPipeline (EmpresaId);
    CREATE INDEX IX_LeadPipeline_PipelineEtapaId ON dbo.LeadPipeline (PipelineEtapaId);

    -- O repositorio checa "este contato ja esta no funil?" antes de inserir; sem indice unico
    -- duas requisicoes simultaneas conseguiriam duplicar o mesmo contato no mesmo funil.
    CREATE UNIQUE INDEX UX_LeadPipeline_Contato_Etapa ON dbo.LeadPipeline (EmpresaId, ContatoId, PipelineEtapaId);
END
GO
