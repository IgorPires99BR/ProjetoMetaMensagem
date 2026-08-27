USE ContactSolutionDB;
GO

BEGIN TRANSACTION;

-- SourceIdAnuncio: id do anuncio (source_id que a Meta manda no referral da PRIMEIRA mensagem
-- de quem chega por Click-to-WhatsApp). Amarrar o flow ao anuncio resolve o furo do gatilho por
-- texto: hoje cada criativo abre o WhatsApp com uma mensagem sugerida diferente e essa mensagem
-- decide o flow -- so que a pessoa pode apagar o texto e escrever o que quiser, e aí cai no flow
-- curinga em vez do que combinava com o anuncio dela.
--
-- NULL = flow sem anuncio amarrado, selecionado pelo gatilho de texto como sempre.
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[Fluxo]') AND name = 'SourceIdAnuncio')
BEGIN
    ALTER TABLE Fluxo ADD SourceIdAnuncio NVARCHAR(60) NULL;
END

COMMIT TRANSACTION;
GO
