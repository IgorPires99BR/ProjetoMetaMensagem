using ProjetoMetaMensagem.Dominio.Common;
using ProjetoMetaMensagem.Dominio.Interfaces.Mediator;
using System;

namespace ProjetoMetaMensagem.Dominio.UseCases.Empresa.ObtemSituacaoSubconta
{
    public class ObtemSituacaoSubcontaCommand : IRequest<Response<ObtemSituacaoSubcontaResult>>
    {
        public ObtemSituacaoSubcontaCommand(Guid empresaId)
        {
            EmpresaId = empresaId;
        }

        public Guid EmpresaId { get; set; }
    }
}
