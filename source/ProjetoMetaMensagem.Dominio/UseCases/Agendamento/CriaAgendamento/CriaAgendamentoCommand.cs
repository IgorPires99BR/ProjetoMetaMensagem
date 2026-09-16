using ProjetoMetaMensagem.Dominio.Common;
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
        public DateTime DataInicio { get; set; }
        public DateTime? DataFim { get; set; }
        public List<Guid> ContatoIds { get; set; }
        public Guid? UsuarioCriacaoId { get; set; }
    }
}
