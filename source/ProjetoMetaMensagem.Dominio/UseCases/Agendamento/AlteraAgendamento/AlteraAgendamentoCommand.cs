using ProjetoMetaMensagem.Dominio.Common;
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
        public List<Guid> ContatoIds { get; set; }

        // Preenchido pelo controller a partir do JWT (null = administrador da plataforma).
        // A empresa "dona" do agendamento e sempre a carregada do banco pelo Id, nunca algo
        // que o cliente mande -- so esse valor decide se ele PODE editar o registro.
        public Guid? EmpresaIdSolicitante { get; set; }
    }
}
