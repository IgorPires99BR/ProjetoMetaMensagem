using ProjetoMetaMensagem.Dominio.Common;
using ProjetoMetaMensagem.Dominio.Entidades;
using ProjetoMetaMensagem.Dominio.Interfaces.Mediator;
using System;
using System.Collections.Generic;

namespace ProjetoMetaMensagem.Dominio.UseCases.Agendamento.AlteraAgendamento
{
    public class AlteraAgendamentoCommand : IRequest<Response<AlteraAgendamentoResult>>
    {
        public Guid Id { get; set; }
        public string Nome { get; set; }
        public Guid TemplateId { get; set; }
        public string TipoRecorrencia { get; set; }
        public DateTime DataInicio { get; set; }
        public DateTime? DataFim { get; set; }
        public DateTime DataReferencia { get; set; }
        public List<Guid> ContatoIds { get; set; }
        public List<AgendamentoVariavelDto> Variaveis { get; set; }

        // Dias da semana (0=Domingo...6=Sabado) em que o agendamento SEMANAL dispara -- ex:
        // segunda/quarta/sexta. Vazio/null = repete a cada 7 dias no dia da DataReferencia.
        public List<int> DiasSemana { get; set; }

        // Dia do mes (1-31) em que o agendamento MENSAL dispara. Null = usa o dia da DataReferencia.
        public int? DiaDoMes { get; set; }

        // Preenchido pelo controller a partir do JWT (null = administrador da plataforma).
        // A empresa "dona" do agendamento e sempre a carregada do banco pelo Id, nunca algo
        // que o cliente mande -- so esse valor decide se ele PODE editar o registro.
        public Guid? EmpresaIdSolicitante { get; set; }
    }
}
