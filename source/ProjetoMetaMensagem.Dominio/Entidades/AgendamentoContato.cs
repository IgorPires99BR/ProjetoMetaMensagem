using System;
using System.ComponentModel.DataAnnotations;

namespace ProjetoMetaMensagem.Dominio.Entidades
{
    // Lista persistente de destinatarios do agendamento -- diferente de CampanhaContato,
    // nao tem Processado/Sucesso por linha porque o mesmo contato e reenviado a cada
    // recorrencia; o resultado de cada execucao fica em AgendamentoExecucao + no log em arquivo.
    public class AgendamentoContato
    {
        [Key]
        public Guid Id { get; set; }

        [Required]
        public Guid AgendamentoId { get; set; }

        [Required]
        public Guid ContatoId { get; set; }
    }
}
