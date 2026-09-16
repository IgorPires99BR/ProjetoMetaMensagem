using ProjetoMetaMensagem.Dominio.Entidades;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ProjetoMetaMensagem.Dominio.Interfaces.Repositorios
{
    public class AgendamentoListItem
    {
        public Guid Id { get; set; }
        public string Nome { get; set; }
        public Guid TemplateId { get; set; }
        public string NomeTemplate { get; set; }
        public string TipoRecorrencia { get; set; }
        public DateTime DataInicio { get; set; }
        public DateTime? DataFim { get; set; }
        public DateTime ProximaExecucao { get; set; }
        public bool Ativo { get; set; }
        public int TotalContatos { get; set; }
        public string DiasSemana { get; set; }
        public int? DiaDoMes { get; set; }
    }

    public interface IAgendamentoRepository
    {
        Task<Guid> Incluir(Agendamento agendamento);
        Task IncluirContatos(List<AgendamentoContato> contatos);
        // Delete + insert dos vinculos, usado na edicao (a lista de contatos pode mudar por completo).
        Task SubstituirContatos(Guid agendamentoId, List<Guid> contatoIds);
        Task<IEnumerable<AgendamentoListItem>> Listar(Guid empresaId);
        Task<Agendamento?> ObterPorId(Guid id, Guid? empresaIdSolicitante);
        Task<IEnumerable<Guid>> ObterContatoIds(Guid agendamentoId);
        Task<int> Atualizar(Agendamento agendamento, Guid? empresaIdSolicitante);
        Task<int> AtualizarStatus(Guid id, bool ativo, Guid? empresaIdSolicitante);
        Task<int> Deletar(Guid id, Guid? empresaIdSolicitante);

        // Usados pelo AgendamentoDispatchJob (roda fora do escopo de um usuario logado).
        Task<IEnumerable<Agendamento>> ObterPendentes(DateTime agora);
        // Reserva por prazo (mesmo padrao de EstadoConversa.ProcessandoAte): devolve false se
        // outra passada do job ja reivindicou esse agendamento e a reserva ainda esta valida.
        Task<bool> ReivindicarAgendamento(Guid id, DateTime prazoProcessamento);
        Task FinalizarExecucao(Guid id, DateTime proximaExecucao, bool ativo);
        Task<Guid> IncluirExecucao(AgendamentoExecucao execucao);
        Task<IEnumerable<AgendamentoExecucao>> ListarExecucoes(Guid agendamentoId, Guid? empresaIdSolicitante);
    }
}
