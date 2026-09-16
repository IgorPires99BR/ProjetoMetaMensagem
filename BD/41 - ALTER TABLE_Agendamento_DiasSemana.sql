USE ContactSolutionDB;
GO

BEGIN TRANSACTION;

-- Recorrencia semanal em dias especificos (ex: segunda, quarta e sexta) e recorrencia mensal
-- num dia do mes explicito, escolhidos na tela em vez de sempre repetirem no mesmo dia da
-- semana/mes da DataReferencia -- ver AgendamentoRecorrencia.cs.
-- DiasSemana: CSV de inteiros 0-6 (0=Domingo...6=Sabado, igual DayOfWeek do .NET). NULL/vazio
-- em SEMANAL cai no comportamento antigo (repete a cada 7 dias, no dia da semana da DataReferencia).
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Agendamento') AND name = 'DiasSemana')
BEGIN
    ALTER TABLE Agendamento ADD DiasSemana NVARCHAR(20) NULL;
END;

-- DiaDoMes: 1-31, usado so quando TipoRecorrencia = MENSAL. NULL cai no comportamento antigo
-- (usa o dia da DataReferencia, clampado pro ultimo dia do mes quando o mes for menor).
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Agendamento') AND name = 'DiaDoMes')
BEGIN
    ALTER TABLE Agendamento ADD DiaDoMes INT NULL;
END;

COMMIT TRANSACTION;
GO
