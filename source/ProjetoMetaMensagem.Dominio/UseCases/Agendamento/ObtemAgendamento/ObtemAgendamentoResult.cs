using ProjetoMetaMensagem.Dominio.Entidades;
using System;
using System.Collections.Generic;

namespace ProjetoMetaMensagem.Dominio.UseCases.Agendamento.ObtemAgendamento
{
    public class ObtemAgendamentoResult
    {
        public Guid Id { get; set; }
        public string Nome { get; set; }
        public Guid TemplateId { get; set; }
        public string TipoRecorrencia { get; set; }
        public DateTime DataInicio { get; set; }
        public DateTime? DataFim { get; set; }
        public DateTime DataReferencia { get; set; }
        public DateTime ProximaExecucao { get; set; }
        public bool Ativo { get; set; }
        public List<Guid> ContatoIds { get; set; }
        public List<AgendamentoVariavelDto> Variaveis { get; set; }
    }
}
