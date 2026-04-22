use ContactSolutionDB

-- Tabelas Dependentes
IF OBJECT_ID('dbo.Template', 'U') IS NOT NULL DROP TABLE dbo.Template;
IF OBJECT_ID('dbo.Templates', 'U') IS NOT NULL DROP TABLE dbo.Templates;

IF OBJECT_ID('dbo.Contato', 'U') IS NOT NULL DROP TABLE dbo.Contato;
IF OBJECT_ID('dbo.Contatos', 'U') IS NOT NULL DROP TABLE dbo.Contatos;

IF OBJECT_ID('dbo.Numero', 'U') IS NOT NULL DROP TABLE dbo.Numero;
IF OBJECT_ID('dbo.Numeros', 'U') IS NOT NULL DROP TABLE dbo.Numeros;

-- Tabelas Pai
IF OBJECT_ID('dbo.Usuario', 'U') IS NOT NULL DROP TABLE dbo.Usuario;
IF OBJECT_ID('dbo.Usuarios', 'U') IS NOT NULL DROP TABLE dbo.Usuarios;

IF OBJECT_ID('dbo.Empresa', 'U') IS NOT NULL DROP TABLE dbo.Empresa;
IF OBJECT_ID('dbo.Empresas', 'U') IS NOT NULL DROP TABLE dbo.Empresas;
GO



