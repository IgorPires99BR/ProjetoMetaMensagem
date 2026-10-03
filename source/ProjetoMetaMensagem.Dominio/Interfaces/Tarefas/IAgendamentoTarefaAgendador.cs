using ProjetoMetaMensagem.Dominio.Entidades;
using System;

namespace ProjetoMetaMensagem.Dominio.Interfaces.Tarefas
{
    // Cada Agendamento tem o proprio job recorrente no HangFire, com o cron da recorrencia
    // escolhida na tela (ver AgendamentoRecorrencia.ExpressaoCron). Interface no Dominio porque
    // o pacote do HangFire so e referenciado pela WebAPI.
    public interface IAgendamentoTarefaAgendador
    {
        // Cria ou atualiza o job do agendamento; se ele estiver inativo, remove.
        void Registrar(Agendamento agendamento);

        void Remover(Guid agendamentoId);

        // Melhor esforco: nao lanca excecao (o proximo acordar do cron pega o disparo atrasado
        // de qualquer forma), porque e chamado depois do Commit dos handlers.
        void ExecutarAgora(Guid agendamentoId);
    }
}
