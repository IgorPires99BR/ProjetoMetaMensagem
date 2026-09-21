USE ContactSolutionDB;
GO

/* SCRIPT DE CARGA IDEMPOTENTE
   Cadastra na Sebrecon os clientes da planilha "CONTA A RECEBER SEBRECON / GILSON 09/2026",
   com o Gilson como dono do cadastro (rode o Y03 antes, que cria a empresa e o usuario).

   Regras da carga:
   - NomeCliente = coluna CLIENTE (como veio na planilha, so com espacos repetidos colapsados);
     NomeContato = "Contato 1" (Nome Proprio).
   - Telefone = so digitos com 55 na frente (formato que a Meta espera -- ver TelefoneHelper).
     So o "Telefone 1" foi usado: Contato guarda um telefone so; o "Telefone 2" ficou de fora.
   - DiaVencimento = DIA da coluna VENCIMENTO (20 ou 30); o mes/ano da planilha nao e guardado --
     a mensagem monta a data no mes da execucao (variavel dataVencimento).
   - ValorFatura = coluna VALOR. Juros: os padroes de Contato (2,00 e 2,00), pois a planilha
     nao traz taxa.
   - Idempotente: um cliente so entra se a Sebrecon ainda nao tem contato com o mesmo
     Telefone + NomeCliente. Pode rodar de novo sem duplicar.

   FICARAM DE FORA (conferir e cadastrar pela tela de Contatos):
   - Sem telefone na planilha: Adega nossa senhora, GIJU CONFECCCOES, GIOVANNA PAES CONGELADOS,
     GOLDEN SILVER, MECADO MARIA RODRIGUES, MERCADO GEANE GIL, MERCADO GIOVANA, RENATO FUCK,
     SDL ELETRO, e a ultima linha (R$ 555,00, sem nome nem contato).
   - Telefone invalido como escrito:
       DROGARIA - GIOVANNA  -> "981589197" (9 digitos, falta o DDD)
       FRAGA CATALIZADORES  -> "94020-6621" (falta o DDD; parece o mesmo numero da Hellen, 11-94020-6621)
       HORTIFRUTI PENTEADO  -> "11910093-9558" (12 digitos; parece digitacao de 11-91093-9558)

   ATENCAO: alguns telefones aparecem em mais de um cliente (mesma pessoa cuida de varias
   empresas): 5511978737005, 5511940206621, 5521964506651, 5511994516053, 5511974418217,
   5511950587210, 5511910939558, 5511939512461.
*/

DECLARE @EmpresaId UNIQUEIDENTIFIER;
SELECT @EmpresaId = Id FROM Empresa WHERE Nome = 'Sebrecon';

IF @EmpresaId IS NULL
BEGIN
    PRINT 'Empresa "Sebrecon" nao encontrada -- rode o Y03 antes deste script.';
    RETURN;
END

DECLARE @UsuarioId UNIQUEIDENTIFIER;
SELECT @UsuarioId = Id FROM Usuario WHERE EmpresaId = @EmpresaId AND Email = 'gilson@sebrecon.com.br';

IF @UsuarioId IS NULL
BEGIN
    PRINT 'Usuario "gilson@sebrecon.com.br" nao encontrado na Sebrecon -- rode o Y03 antes deste script.';
    RETURN;
END

CREATE TABLE #Novos (
    NomeCliente   NVARCHAR(255) NOT NULL,
    NomeContato   NVARCHAR(255) NOT NULL,
    Telefone      NVARCHAR(50)  NOT NULL,
    DiaVencimento INT           NOT NULL,
    ValorFatura   DECIMAL(10,2) NOT NULL
);

INSERT INTO #Novos (NomeCliente, NomeContato, Telefone, DiaVencimento, ValorFatura) VALUES
    (N'ACS comercio Adriano', N'Adriano', '5511988012391', 20, 355.00),
    (N'A2S TRANPORTES', N'Ademir', '5511978737005', 20, 355.00),
    (N'ACS NON S TESTING ELEINE 7x50', N'Elaine', '5511990257812', 20, 1005.00),
    (N'AMERICA - XIS ALUGUEL', N'Ademir', '5511978737005', 20, 505.00),
    (N'Andere Serviços adminsitrativo', N'Solange', '5511984672645', 20, 655.00),
    (N'APHA CATALISADOR 3x50', N'Julia', '5511940206621', 20, 505.00),
    (N'AUREO COMERCIO 7X50', N'Edu', '5511999966686', 20, 655.00),
    (N'B. Arquitetura', N'Beatriz', '5511976617111', 20, 305.00),
    (N'BELLANAAH TRANSPORTE', N'Nagila', '5511913375093', 20, 355.00),
    (N'BORGATTO 13x 50', N'Ana', '5511947864135', 20, 1005.00),
    (N'BOUTIQUE - SIMONE', N'Ademir', '5511978737005', 20, 355.00),
    (N'C & S MODAS LTDA', N'Adelaide', '5511986810976', 20, 355.00),
    (N'CAMILA MARKETING- 50,00', N'Camila', '5511941413389', 20, 355.00),
    (N'Carbocase design', N'Patricia', '5521964506651', 30, 455.00),
    (N'CATALISADORES WEB', N'Hellen', '5511940206621', 20, 355.00),
    (N'CENTRO DIAGNOSTICOS 1X', N'Ceila', '5511974418217', 20, 405.00),
    (N'CLA - PROMOTOR', N'Cleiton', '5511976934175', 20, 405.00),
    (N'Conectadosnet', N'Kátia', '5511961795426', 30, 205.00),
    (N'DNA- PRECISION', N'Renato', '5511994516053', 20, 305.00),
    (N'EFA ENGENHARIA', N'Fernando', '5511969678948', 20, 355.00),
    (N'EL CLIMA , SISTEMA AR CONDICIONADO', N'Ezequiel', '5511998955736', 20, 355.00),
    (N'ENIKA ACAI', N'Karol', '5511948925819', 20, 355.00),
    (N'F.F LAVA RAPIDO 1/50', N'Fabio', '5511962664892', 20, 405.00),
    (N'Felp Serviços Graficos', N'Fernanda', '5511972864920', 30, 125.00),
    (N'Fenix Pint', N'Genival', '5511981070881', 20, 125.00),
    (N'GG TRONIC', N'Arnaldo', '5511950029988', 20, 355.00),
    (N'GRG ASSESSROIA EMPRESARIAL', N'Ronaldo', '5511963116761', 20, 355.00),
    (N'H.J LABORATORIO -HENRIQUE', N'Henrique', '5511974418217', 30, 850.00),
    (N'HORTIFRUTI PAULISTANO 7x50 350,00', N'Rafael', '5511910939558', 20, 705.00),
    (N'Hugo Filmagem MEI', N'Hugo', '5511959800454', 20, 125.00),
    (N'Igor George- IGM TECNOLOGIA', N'Igor', '5511980537452', 20, 125.00),
    (N'Igreja Evangelica Crista da promessa', N'Davi', '5511943128732', 20, 505.00),
    (N'IMPERIUM CAR', N'Jaqueline', '5511939512461', 30, 355.00),
    (N'Inka Consultoria', N'Manuelito', '5511999670148', 20, 355.00),
    (N'IPLA- INTELLOGIS', N'Andre', '5511984668722', 20, 355.00),
    (N'J&M MULTIMARCAS', N'Murilo', '5511981718953', 20, 355.00),
    (N'JLC CAR', N'Carol', '5511958634299', 20, 355.00),
    (N'JWM Manutencao', N'Talita', '5511972951312', 20, 355.00),
    (N'Kaiomax Automotivos 8x50 350,00', N'Poliana', '5511981214932', 20, 755.00),
    (N'KF MONITORAMENTO', N'Kirk', '5511950587210', 30, 405.00),
    (N'L & B SANTOS SERVIÇOS', N'Luiz', '5511947861567', 20, 355.00),
    (N'L & D SOLUÇOES IMOBILIARIA XIS', N'Derick', '5511971890731', 20, 455.00),
    (N'Labadessa Serviços', N'Danielle', '5511919192498', 20, 355.00),
    (N'LAREDO ART', N'Patricia', '5521964506651', 30, 405.00),
    (N'LAVI, ENGENHARIA E ADMISNTRAÇÃO', N'Leonardo', '5511914264623', 20, 405.00),
    (N'LD ANGIO TRICOLOGIA', N'Laisa', '5511966317770', 20, 355.00),
    (N'LEEV PRECISION', N'Renato', '5511994516053', 20, 305.00),
    (N'LEONARDO GASPAR', N'Leonardo', '5511999531428', 20, 405.00),
    (N'MAAIBUS TRANSPORTE', N'Lia', '5511972654970', 20, 355.00),
    (N'MAXIMUS - 1 x50', N'Marcelo', '5511952193318', 20, 355.00),
    (N'Maxwell MEI', N'Maxwell', '5511991174246', 20, 125.00),
    (N'MED - DNA PRECISION', N'Renato', '5511994516053', 20, 305.00),
    (N'MEIRE SERVIÇOS ADMINISTRATIVOS', N'Meire', '5511988319801', 20, 305.00),
    (N'MG Transporte - Gilson', N'Marisa', '5511947172128', 20, 355.00),
    (N'OLIVEIRA E CONSTRUCAO 7x50', N'Paulino', '5511948864401', 20, 705.00),
    (N'Oncoral - dentista', N'Fabio', '5511998996300', 20, 355.00),
    (N'PEDAGOGA NINA', N'Sonia', '5511988355443', 20, 125.00),
    (N'PLANETA LOGISTICA', N'Luciano', '5511950715321', 20, 355.00),
    (N'PREFERITA TRNASPORTE', N'Rafael', '5511910939558', 20, 405.00),
    (N'R & F TRANSPÓRTES & SERVIÇOS', N'Felipe', '5531996727763', 20, 355.00),
    (N'RECOSME REPRESENTACAO', N'Jorge', '5511975800820', 20, 405.00),
    (N'REI DAS MASSAS ARTESANAL', N'Priscila', '5511962258563', 30, 355.00),
    (N'RENZO SHOOTER CLUB', N'Alex', '5511947785507', 20, 125.00),
    (N'RESTAURANTE CARVALHO', N'Thiago', '5511994906724', 20, 355.00),
    (N'Ronaldinho chaveiro', N'Davi', '5511971090553', 20, 125.00),
    (N'S8 IT SERVIÇOS', N'Fabio', '5511989385012', 20, 255.00),
    (N'SHINY LIFE - VANDA 350,00', N'Vanda', '5511977008877', 20, 405.00),
    (N'SOLAR TECNOLOGIA 40/50 700,00', N'Kirk', '5511950587210', 20, 2705.00),
    (N'SOLIVEI TREINAMENTO', N'Luciana', '5511983411255', 20, 355.00),
    (N'Tabacaria RAFAEL', N'Rafael', '5511976010139', 20, 155.00),
    (N'TFA GUINCHOS( 02) um gratis', N'Cristiane', '5511985261177', 30, 355.00),
    (N'Thunga Buger 3x50 350,00', N'Poliana', '5511953103423', 20, 505.00),
    (N'Trade traduçoes', N'Edvaldo', '5511981608501', 20, 375.00),
    (N'VIBRASER TERAPIAS', N'Patricia', '5511982589468', 20, 355.00),
    (N'VKR- INSTALACAO - VICTOR GOLA', N'Victor', '5511982660043', 30, 355.00),
    (N'VRIVACRED', N'Douglas', '5511954816778', 20, 405.00),
    (N'WELLNES PRO ESUDIO', N'Carol', '5511939512461', 20, 355.00);

DECLARE @NaPlanilha INT = (SELECT COUNT(*) FROM #Novos);

-- Juros iguais aos padroes de Contato (Contato.TaxaJurosPadrao / TaxaJurosMensalPadrao).
INSERT INTO Contato (Id, UsuarioId, EmpresaId, Telefone, NomeContato, NomeCliente,
                     DiaVencimento, TaxaJuros, TaxaJurosMensal, ValorFatura, DataCriacao)
SELECT NEWID(), @UsuarioId, @EmpresaId, n.Telefone, n.NomeContato, n.NomeCliente,
       n.DiaVencimento, 2.00, 2.00, n.ValorFatura, GETDATE()
FROM #Novos n
WHERE NOT EXISTS (
    SELECT 1 FROM Contato c
    WHERE c.EmpresaId = @EmpresaId AND c.Telefone = n.Telefone AND c.NomeCliente = n.NomeCliente
);

DECLARE @Inseridos INT = @@ROWCOUNT;

PRINT CONCAT('Clientes na carga: ', @NaPlanilha, ' | inseridos agora: ', @Inseridos,
             ' | ja existiam: ', @NaPlanilha - @Inseridos);

DROP TABLE #Novos;
GO
