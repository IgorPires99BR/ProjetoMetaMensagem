using System;

namespace ProjetoMetaMensagem.Dominio.UseCases.Campanha.ListaCampanha
{
    public class ListaCampanhaResult
    {
        public Guid Id { get; set; }
        public string Nome { get; set; }
        public DateTime DataAgendamento { get; set; }
        public string Status { get; set; }
        public int TotalContatos { get; set; }
        public int Processados { get; set; }
    }
}
