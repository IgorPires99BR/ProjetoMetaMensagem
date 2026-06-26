USE ContactSolutionDB;
GO

IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[ContatoTag]') AND type in (N'U'))
BEGIN
    CREATE TABLE ContatoTag (
        ContatoId UNIQUEIDENTIFIER NOT NULL,
        TagId UNIQUEIDENTIFIER NOT NULL,
        DataCriacao DATETIME DEFAULT GETDATE(),

        CONSTRAINT PK_ContatoTag PRIMARY KEY (ContatoId, TagId),
        CONSTRAINT FK_ContatoTag_Contato FOREIGN KEY (ContatoId) REFERENCES Contato(Id),
        CONSTRAINT FK_ContatoTag_Tag FOREIGN KEY (TagId) REFERENCES Tag(Id)
    );
END
GO
