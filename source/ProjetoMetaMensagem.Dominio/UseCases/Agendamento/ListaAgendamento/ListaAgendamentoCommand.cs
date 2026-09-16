using ProjetoMetaMensagem.Dominio.Common;
using ProjetoMetaMensagem.Dominio.Interfaces.Mediator;
using System;
using System.Collections.Generic;

namespace ProjetoMetaMensagem.Dominio.UseCases.Agendamento.ListaAgendamento
{
    public class ListaAgendamentoCommand : IRequest<Response<List<ListaAgendamentoResult>>>
    {
        public Guid EmpresaId { get; set; }
    }
}
