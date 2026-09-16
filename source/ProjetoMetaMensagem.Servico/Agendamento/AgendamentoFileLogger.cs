using Microsoft.Extensions.Configuration;
using Serilog;
using Serilog.Core;
using System;
using System.IO;

namespace ProjetoMetaMensagem.Servico.Agendamento
{
    // Log em arquivo dedicado ao AgendamentoDispatchJob, separado do ILogger<T> usado no
    // resto da API -- o pedido era um arquivo no servidor so com o resultado (sucesso/erro)
    // de cada mensagem agendada, sem misturar com o log geral nem depender de configurar
    // um sink global so por causa desse job.
    public class AgendamentoFileLogger
    {
        private readonly Logger _logger;

        public AgendamentoFileLogger(IConfiguration configuration)
        {
            var diretorio = configuration["AgendamentoWorker:DiretorioLog"] ?? "logs/agendamentos";
            var caminho = Path.Combine(diretorio, "agendamento-.log");

            _logger = new LoggerConfiguration()
                .WriteTo.File(caminho, rollingInterval: RollingInterval.Day, retainedFileCountLimit: 30)
                .CreateLogger();
        }

        public void Sucesso(Guid agendamentoId, Guid contatoId, string telefone)
        {
            _logger.Information(
                "SUCESSO agendamento={AgendamentoId} contato={ContatoId} telefone={Telefone}",
                agendamentoId, contatoId, telefone);
        }

        public void Falha(Guid agendamentoId, Guid contatoId, string telefone, string motivo)
        {
            _logger.Error(
                "FALHA agendamento={AgendamentoId} contato={ContatoId} telefone={Telefone} motivo={Motivo}",
                agendamentoId, contatoId, telefone, motivo);
        }

        public void InicioExecucao(Guid agendamentoId, string nome, int totalContatos)
        {
            _logger.Information(
                "INICIO agendamento={AgendamentoId} nome={Nome} totalContatos={TotalContatos}",
                agendamentoId, nome, totalContatos);
        }

        public void FimExecucao(Guid agendamentoId, int sucessos, int falhas)
        {
            _logger.Information(
                "FIM agendamento={AgendamentoId} sucessos={Sucessos} falhas={Falhas}",
                agendamentoId, sucessos, falhas);
        }

        public void ErroInesperado(Guid agendamentoId, Exception excecao)
        {
            _logger.Error(excecao, "ERRO_INESPERADO agendamento={AgendamentoId}", agendamentoId);
        }
    }
}
