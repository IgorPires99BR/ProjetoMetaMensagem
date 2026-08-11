-- Vincula um Fluxo a um Numero especifico (opcional). NULL continua significando
-- "vale para todos os numeros da empresa", preservando os fluxos ja existentes sem
-- precisar de migracao de dados.
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('dbo.Fluxo') AND name = 'NumeroId')
BEGIN
    ALTER TABLE Fluxo ADD NumeroId UNIQUEIDENTIFIER NULL
        CONSTRAINT FK_Fluxo_Numero FOREIGN KEY REFERENCES Numero(Id);

    CREATE INDEX IX_Fluxo_NumeroId ON Fluxo(NumeroId);
END
