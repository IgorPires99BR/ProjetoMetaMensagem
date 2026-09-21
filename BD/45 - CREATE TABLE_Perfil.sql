USE ContactSolutionDB;
GO

-- Estrutura de acesso modular: cada Usuario pode ter um Perfil (por empresa), e cada
-- Perfil define quais telas do menu ele enxerga (PerfilTela.TelaChave bate com o "id" de
-- cada item em shared/menu.ts no front). Usuario.PerfilId nulo = comportamento legado
-- (admin ve tudo, operador cai no allowlist fixo do front) -- nao quebra quem ja existe.
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='Perfil' AND xtype='U')
BEGIN
    CREATE TABLE Perfil (
        Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
        EmpresaId UNIQUEIDENTIFIER NOT NULL,
        Nome NVARCHAR(100) NOT NULL,
        DataCriacao DATETIME DEFAULT GETDATE(),
        CONSTRAINT FK_Perfil_Empresa FOREIGN KEY (EmpresaId) REFERENCES Empresa(Id)
    );
END
GO

IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='PerfilTela' AND xtype='U')
BEGIN
    CREATE TABLE PerfilTela (
        PerfilId UNIQUEIDENTIFIER NOT NULL,
        TelaChave NVARCHAR(50) NOT NULL,
        CONSTRAINT PK_PerfilTela PRIMARY KEY (PerfilId, TelaChave),
        CONSTRAINT FK_PerfilTela_Perfil FOREIGN KEY (PerfilId) REFERENCES Perfil(Id) ON DELETE CASCADE
    );
END
GO

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[Usuario]') AND name = 'PerfilId')
    ALTER TABLE Usuario ADD PerfilId UNIQUEIDENTIFIER NULL;
GO

IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_Usuario_Perfil')
    ALTER TABLE Usuario ADD CONSTRAINT FK_Usuario_Perfil FOREIGN KEY (PerfilId) REFERENCES Perfil(Id);
GO
