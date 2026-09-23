USE ContactSolutionDB;
GO

-- Uma linha por disparo de template marcado GeraCobranca = 1 (ver BD/47): a cobranca que quem
-- revende a plataforma (ex: Sebrecon) faz ao PROPRIO cliente dela (ex: um devedor do Gilson),
-- via link de pagamento dentro do template. Sem relacao nenhuma com a tabela Assinatura, que e
-- a cobranca da assinatura SaaS da Contact Solution.
--
-- Valor/DataVencimento sao SNAPSHOT do Contato no momento do disparo (ContatoRepository.
-- ValorFatura/DiaVencimento), nao um JOIN ao vivo -- se o cadastro do contato mudar depois,
-- a cobranca ja disparada nao pode mudar de valor retroativamente.
--
-- Confirmacao de pagamento por enquanto e manual (endpoint marcar-paga): o link de pagamento
-- ainda e o mesmo checkout compartilhado da Contact Solution (nao da pra casar automaticamente
-- via webhook da Cakto sem o link ser especifico por contato -- ver UtmContentCakto, deixado
-- pronto pra quando isso mudar).
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='CobrancaCliente' AND xtype='U')
BEGIN
    CREATE TABLE CobrancaCliente (
        Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
        EmpresaId UNIQUEIDENTIFIER NOT NULL,
        ContatoId UNIQUEIDENTIFIER NOT NULL,
        TemplateId UNIQUEIDENTIFIER NOT NULL,
        HistoricoDisparoId UNIQUEIDENTIFIER NOT NULL,
        Valor DECIMAL(10,2) NOT NULL,
        DataVencimento DATETIME NOT NULL,
        Status NVARCHAR(20) NOT NULL DEFAULT ('PENDENTE'),
        DataPagamento DATETIME NULL,
        -- Chave de correlacao com o webhook da Cakto (utm_content do link de pagamento), pra
        -- quando o link deixar de ser compartilhado. Sem uso ainda -- marcacao e manual.
        UtmContentCakto NVARCHAR(100) NULL,
        EventoIdCakto NVARCHAR(100) NULL,
        DataCriacao DATETIME NOT NULL DEFAULT (GETDATE()),
        DataAtualizacao DATETIME NULL,

        CONSTRAINT FK_CobrancaCliente_Empresa FOREIGN KEY (EmpresaId) REFERENCES Empresa(Id),
        CONSTRAINT FK_CobrancaCliente_Contato FOREIGN KEY (ContatoId) REFERENCES Contato(Id),
        CONSTRAINT FK_CobrancaCliente_Template FOREIGN KEY (TemplateId) REFERENCES Template(Id),
        CONSTRAINT FK_CobrancaCliente_HistoricoDisparo FOREIGN KEY (HistoricoDisparoId) REFERENCES HistoricoDisparo(Id)
    );
END
GO

-- Consulta do futuro job de recorrencia (cobrancas pendentes/vencidas por empresa) e da tela de
-- gestao: por status dentro da empresa, ordenado por vencimento.
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_CobrancaCliente_Empresa_Status' AND object_id = OBJECT_ID(N'[dbo].[CobrancaCliente]'))
    CREATE INDEX IX_CobrancaCliente_Empresa_Status ON CobrancaCliente (EmpresaId, Status, DataVencimento);
GO
