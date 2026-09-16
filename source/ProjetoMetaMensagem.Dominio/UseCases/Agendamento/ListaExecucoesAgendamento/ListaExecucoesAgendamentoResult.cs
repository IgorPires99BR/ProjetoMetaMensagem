using System;

namespace ProjetoMetaMensagem.Dominio.UseCases.Agendamento.ListaExecucoesAgendamento
{
    public class ListaExecucoesAgendamentoResult
    {
        public Guid Id { get; set; }
        public DateTime DataExecucao { get; set; }
        public int TotalContatos { get; set; }
        public int Sucessos { get; set; }
        public int Falhas { get; set; }
        public string Status { get; set; }
    }
}
