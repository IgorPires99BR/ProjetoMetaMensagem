namespace ProjetoMetaMensagem.Dominio.UseCases.Agendamento.ProcessaAgendamento
{
    public class ProcessaAgendamentoResult
    {
        public int TotalAgendamentos { get; set; }
        public int TotalContatos { get; set; }
        public int Sucessos { get; set; }
        public int Falhas { get; set; }
    }
}
