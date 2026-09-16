using ProjetoMetaMensagem.Dominio.Common;
using ProjetoMetaMensagem.Dominio.Interfaces.Mediator;

namespace ProjetoMetaMensagem.Dominio.UseCases.Agendamento.ProcessaAgendamento
{
    // Varre TODOS os agendamentos pendentes (de todas as empresas) e processa cada um --
    // sem parametro de entrada de proposito. Projeto pequeno, uma unica tarefa do HangFire
    // (AgendamentoMensagemTarefa) e responsavel pela varredura inteira a cada execucao, em vez
    // de uma chamada por agendamento. Agrupamento por EmpresaId acontece dentro do Handler.
    public class ProcessaAgendamentoCommand : IRequest<Response<ProcessaAgendamentoResult>>
    {
    }
}
