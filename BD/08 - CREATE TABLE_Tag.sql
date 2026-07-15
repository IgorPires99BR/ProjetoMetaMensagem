USE ContactSolutionDB;
GO

IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Tag]') AND type in (N'U'))
BEGIN
    CREATE TABLE Tag (
        Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
        EmpresaId UNIQUEIDENTIFIER NOT NULL,
        Nome NVARCHAR(100) NOT NULL,
        Cor NVARCHAR(7) DEFAULT '#3D6EE8',
        DataCriacao DATETIME DEFAULT GETDATE(),

        CONSTRAINT FK_Tag_Empresa FOREIGN KEY (EmpresaId) REFERENCES Empresa(Id)
    );
END
GO
