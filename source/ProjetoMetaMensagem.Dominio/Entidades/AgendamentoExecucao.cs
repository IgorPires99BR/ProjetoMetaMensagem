using System;
using System.ComponentModel.DataAnnotations;

namespace ProjetoMetaMensagem.Dominio.Entidades
{
    // Um registro por passada do job em que o agendamento estava devido. O detalhe por
    // contato (sucesso/erro/motivo) vai pro arquivo de log e pro HistoricoDisparo -- aqui
    // fica so o resumo, pra alimentar a tela de historico sem reprocessar o log.
    public class AgendamentoExecucao
    {
        public const string Concluida = "CONCLUIDA";
        public const string Erro = "ERRO";

        public AgendamentoExecucao()
        {
            Id = Guid.NewGuid();
            DataExecucao = DateTime.Now;
        }

        [Key]
        public Guid Id { get; set; }

        [Required]
        public Guid AgendamentoId { get; set; }

        public DateTime DataExecucao { get; set; }

        public int TotalContatos { get; set; }

        public int Sucessos { get; set; }

        public int Falhas { get; set; }

        [MaxLength(20)]
        public string Status { get; set; }
    }
}
