USE ContactSolutionDB;
GO

-- Enquanto as subcontas nao estao prontas, a empresa pode receber pela conta Asaas da propria
-- Contact Solution: o Pix sai sem split e o valor inteiro fica na conta da plataforma. O acerto
-- com a empresa e feito por fora, por isso cada cobranca guarda onde o dinheiro caiu.
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[DadosBancariosEmpresa]') AND name = 'RecebimentoNaPlataforma')
    ALTER TABLE DadosBancariosEmpresa ADD RecebimentoNaPlataforma BIT NOT NULL
        CONSTRAINT DF_DadosBancariosEmpresa_RecebimentoNaPlataforma DEFAULT (0);
GO

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[CobrancaCliente]') AND name = 'RecebidaNaPlataforma')
    ALTER TABLE CobrancaCliente ADD RecebidaNaPlataforma BIT NOT NULL
        CONSTRAINT DF_CobrancaCliente_RecebidaNaPlataforma DEFAULT (0);
GO
