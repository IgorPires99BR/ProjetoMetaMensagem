using ProjetoMetaMensagem.Dominio.Common;
using ProjetoMetaMensagem.Dominio.Interfaces.Mediator;
using System;

namespace ProjetoMetaMensagem.Dominio.UseCases.Agendamento.ProcessaAgendamento
{
    // Processa UM agendamento: cada Agendamento tem o proprio job recorrente no HangFire (ver
    // IAgendamentoTarefaAgendador), que dispara este comando com o id dele no horario do cron.
    public class ProcessaAgendamentoCommand : IRequest<Response<ProcessaAgendamentoResult>>
    {
        public Guid AgendamentoId { get; set; }
    }
}
