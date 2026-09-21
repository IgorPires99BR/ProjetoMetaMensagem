using ProjetoMetaMensagem.Dominio.Common;
using ProjetoMetaMensagem.Dominio.Interfaces.Mediator;
using System;
using System.Collections.Generic;

namespace ProjetoMetaMensagem.Dominio.UseCases.Parametro.ObtemParametro
{
    public class ObtemParametroCommand : IRequest<Response<List<ObtemParametroResult>>>
    {
        public ObtemParametroCommand(Guid empresaId)
        {
            EmpresaId = empresaId;
        }

        public Guid EmpresaId { get; set; }
    }
}
