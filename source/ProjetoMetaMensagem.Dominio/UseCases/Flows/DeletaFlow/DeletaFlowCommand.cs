using ProjetoMetaMensagem.Dominio.Common;
using ProjetoMetaMensagem.Dominio.Interfaces.Mediator;
using System;

namespace ProjetoMetaMensagem.Dominio.UseCases.Flows.DeletaFlow
{
    public class DeletaFlowCommand : IRequest<Response<DeletaFlowResult>>
    {
        public DeletaFlowCommand(Guid id)
        {
            Id = id;
        }

        public Guid Id { get; set; }
    }
}
