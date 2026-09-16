using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ProjetoMetaMensagem.Dominio.Entidades
{
    public class Agendamento
    {
        public const string Diaria = "DIARIA";
        public const string Semanal = "SEMANAL";
        public const string Mensal = "MENSAL";

        public Agendamento()
        {
            Id = Guid.NewGuid();
            Ativo = true;
            DataCriacao = DateTime.Now;
        }

        [Key]
        public Guid Id { get; set; }

        [Required]
        public Guid EmpresaId { get; set; }

        [Required]
        [MaxLength(200)]
        public string Nome { get; set; }

        [Required]
        public Guid TemplateId { get; set; }

        [Required]
        [MaxLength(20)]
        public string TipoRecorrencia { get; set; }

        public DateTime DataInicio { get; set; }

        public DateTime? DataFim { get; set; }

        // Data/hora do 1o disparo e ancora de horario da recorrencia (hora do dia, e no caso
        // mensal tambem o dia do mes) -- DataInicio/DataFim sao so a janela de vigencia.
        public DateTime DataReferencia { get; set; }

        public DateTime ProximaExecucao { get; set; }

        public bool Ativo { get; set; }

        // Reserva de processamento por prazo (mesmo padrao de EstadoConversa.ProcessandoAte,
        // migration 36): evita duas passadas do job disparando o mesmo agendamento ao mesmo
        // tempo sem precisar de lock de banco.
        public DateTime? ProcessandoAte { get; set; }

        public Guid? UsuarioCriacaoId { get; set; }

        public DateTime DataCriacao { get; set; }

        public DateTime? DataAtualizacao { get; set; }

        public string VariaveisJson { get; set; }

        [NotMapped]
        public List<AgendamentoVariavelDto> Variaveis
        {
            get => string.IsNullOrEmpty(VariaveisJson)
                ? new List<AgendamentoVariavelDto>()
                : JsonConvert.DeserializeObject<List<AgendamentoVariavelDto>>(VariaveisJson) ?? new List<AgendamentoVariavelDto>();
            set => VariaveisJson = JsonConvert.SerializeObject(value);
        }
    }
}
