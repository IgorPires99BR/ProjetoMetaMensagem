USE ContactSolutionDB;
GO

BEGIN TRANSACTION;

-- Agendamento recorrente de disparo de template (diario/semanal/mensal), processado pelo
-- HangFire (AgendamentoDispatchJob varre esta tabela a cada 5 min). A recorrencia e
-- calculada so a partir de DataInicio (hora/dia-da-semana/dia-do-mes de referencia) --
-- sem colunas separadas de "dia da semana" ou "dia do mes", ver AgendamentoRecorrencia.cs.
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='Agendamento' AND xtype='U')
BEGIN
    CREATE TABLE Agendamento (
        Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
        EmpresaId UNIQUEIDENTIFIER NOT NULL,
        Nome NVARCHAR(200) NOT NULL,
        TemplateId UNIQUEIDENTIFIER NOT NULL,
        TipoRecorrencia NVARCHAR(20) NOT NULL,
        DataInicio DATETIME NOT NULL,
        DataFim DATETIME NULL,
        ProximaExecucao DATETIME NOT NULL,
        Ativo BIT NOT NULL DEFAULT 1,
        -- Reserva de processamento por prazo, mesmo padrao de EstadoConversa.ProcessandoAte
        -- (migration 36): evita duas passadas do job disparando o mesmo agendamento ao mesmo
        -- tempo sem precisar de lock de banco. Se o processo cair no meio, a reserva expira
        -- sozinha e a proxima passada retoma.
        ProcessandoAte DATETIME NULL,
        UsuarioCriacaoId UNIQUEIDENTIFIER NULL,
        DataCriacao DATETIME DEFAULT GETDATE(),
        DataAtualizacao DATETIME NULL,
        CONSTRAINT FK_Agendamento_Empresa FOREIGN KEY (EmpresaId) REFERENCES Empresa(Id),
        CONSTRAINT FK_Agendamento_Template FOREIGN KEY (TemplateId) REFERENCES Template(Id)
    );
END;

-- Lista persistente de destinatarios: diferente de CampanhaContato, nao tem Processado/Sucesso
-- por linha porque o mesmo contato e reenviado a cada recorrencia -- o resultado de cada
-- execucao fica em AgendamentoExecucao (resumo) + no arquivo de log (detalhe por contato).
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='AgendamentoContato' AND xtype='U')
BEGIN
    CREATE TABLE AgendamentoContato (
        Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
        AgendamentoId UNIQUEIDENTIFIER NOT NULL,
        ContatoId UNIQUEIDENTIFIER NOT NULL,
        CONSTRAINT FK_AgendamentoContato_Agendamento FOREIGN KEY (AgendamentoId) REFERENCES Agendamento(Id) ON DELETE CASCADE,
        CONSTRAINT FK_AgendamentoContato_Contato FOREIGN KEY (ContatoId) REFERENCES Contato(Id)
    );
END;

-- Um registro por passada do job em que o agendamento estava devido, pra alimentar a tela
-- de historico sem precisar reprocessar o arquivo de log.
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='AgendamentoExecucao' AND xtype='U')
BEGIN
    CREATE TABLE AgendamentoExecucao (
        Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
        AgendamentoId UNIQUEIDENTIFIER NOT NULL,
        DataExecucao DATETIME NOT NULL DEFAULT GETDATE(),
        TotalContatos INT NOT NULL DEFAULT 0,
        Sucessos INT NOT NULL DEFAULT 0,
        Falhas INT NOT NULL DEFAULT 0,
        Status NVARCHAR(20) NOT NULL,
        CONSTRAINT FK_AgendamentoExecucao_Agendamento FOREIGN KEY (AgendamentoId) REFERENCES Agendamento(Id) ON DELETE CASCADE
    );
END;

COMMIT TRANSACTION;
GO
