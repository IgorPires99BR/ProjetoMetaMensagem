using System;
using System.Threading.Tasks;

namespace ProjetoMetaMensagem.Dominio.Interfaces.Tarefas
{
    public interface IAgendamentoMensagemTarefa
    {
        Task Executar(Guid agendamentoId);
    }
}
