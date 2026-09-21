using ProjetoMetaMensagem.Dominio.Common;
using ProjetoMetaMensagem.Dominio.Interfaces.Mediator;
using System;

namespace ProjetoMetaMensagem.Dominio.UseCases.Parametro.DeletaParametro
{
    public class DeletaParametroCommand : IRequest<Response<DeletaParametroResult>>
    {
        public Guid Id { get; set; }
        public Guid? EmpresaIdSolicitante { get; set; }
    }
}
