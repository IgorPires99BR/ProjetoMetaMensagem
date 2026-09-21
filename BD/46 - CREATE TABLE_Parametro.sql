USE ContactSolutionDB;
GO

-- Parametros por empresa, reutilizaveis como variavel de template tanto na tela de Disparos
-- quanto na de Agendamentos.
--   Tipo = 'FIXO'          -> Valor e o proprio texto (ex: chave PIX, nome do responsavel).
--   Tipo = 'CAMPO_CONTATO' -> Valor e a chave de um campo do Contato, resolvido por
--                             destinatario a cada envio (ex: valorFatura, dataVencimento).
-- Nome e unico dentro da empresa: e como o operador reconhece o parametro no seletor.
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='Parametro' AND xtype='U')
BEGIN
    CREATE TABLE Parametro (
        Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
        EmpresaId UNIQUEIDENTIFIER NOT NULL,
        Nome NVARCHAR(100) NOT NULL,
        Descricao NVARCHAR(255) NULL,
        Tipo NVARCHAR(20) NOT NULL,
        Valor NVARCHAR(500) NOT NULL,
        DataCriacao DATETIME DEFAULT GETDATE(),
        CONSTRAINT FK_Parametro_Empresa FOREIGN KEY (EmpresaId) REFERENCES Empresa(Id),
        CONSTRAINT UQ_Parametro_Empresa_Nome UNIQUE (EmpresaId, Nome)
    );
END
GO
