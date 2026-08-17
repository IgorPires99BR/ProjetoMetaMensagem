USE ContactSolutionDB;
GO

-- De onde veio o cliente que pagou. A Cakto repassa no webhook os UTMs do checkout e os
-- identificadores do Facebook (fbc = clique no anuncio, fbp = navegador). Sem gravar isso na
-- hora da venda, nao ha como saber depois qual anuncio trouxe qual cliente -- o dado nao volta.
--
-- fbc/fbp sao tambem o que a Conversions API da Meta pede para casar a venda com o anuncio, o
-- que fica muito mais preciso do que so o pixel no navegador.
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[Assinatura]') AND name = 'UtmSource')
BEGIN
    ALTER TABLE dbo.Assinatura ADD
        UtmSource        NVARCHAR(200) NULL,
        UtmMedium        NVARCHAR(200) NULL,
        UtmCampaign      NVARCHAR(200) NULL,
        UtmTerm          NVARCHAR(200) NULL,
        UtmContent       NVARCHAR(200) NULL,
        Sck              NVARCHAR(200) NULL,
        Fbc              NVARCHAR(300) NULL,
        Fbp              NVARCHAR(300) NULL,
        RefIdCakto       NVARCHAR(60)  NULL,
        MetodoPagamento  NVARCHAR(60)  NULL;
END
GO
