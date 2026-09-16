using System;

namespace ProjetoMetaMensagem.Dominio.UseCases.Agendamento.ListaAgendamento
{
    public class ListaAgendamentoResult
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
    }
}
