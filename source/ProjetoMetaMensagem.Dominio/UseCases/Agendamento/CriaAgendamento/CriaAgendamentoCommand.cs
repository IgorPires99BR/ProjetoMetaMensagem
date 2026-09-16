using ProjetoMetaMensagem.Dominio.Common;
using ProjetoMetaMensagem.Dominio.Entidades;
using ProjetoMetaMensagem.Dominio.Interfaces.Mediator;
using System;
using System.Collections.Generic;

namespace ProjetoMetaMensagem.Dominio.UseCases.Agendamento.CriaAgendamento
{
    public class CriaAgendamentoCommand : IRequest<Response<CriaAgendamentoResult>>
    {
        public Guid EmpresaId { get; set; }
        public string Nome { get; set; }
        public Guid TemplateId { get; set; }
        // DIARIA / SEMANAL / MENSAL
        public string TipoRecorrencia { get; set; }
        // Janela de vigencia (datas) em que o agendamento pode disparar.
        public DateTime DataInicio { get; set; }
        public DateTime? DataFim { get; set; }
        // Data/hora do 1o disparo e ancora de horario da recorrencia.
        public DateTime DataReferencia { get; set; }
        public List<Guid> ContatoIds { get; set; }
        public Guid? UsuarioCriacaoId { get; set; }
        public List<AgendamentoVariavelDto> Variaveis { get; set; }

        // Dias da semana (0=Domingo...6=Sabado) em que o agendamento SEMANAL dispara -- ex:
        // segunda/quarta/sexta. Vazio/null = repete a cada 7 dias no dia da DataReferencia.
        public List<int> DiasSemana { get; set; }

        // Dia do mes (1-31) em que o agendamento MENSAL dispara. Null = usa o dia da DataReferencia.
        public int? DiaDoMes { get; set; }
    }
}
