using ProjetoMetaMensagem.Dominio.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjetoMetaMensagem.Dominio.Interfaces.Mediator
{ 
    public interface IRequestHandler<in TRequest, TResponse>
        where TRequest : IRequest<TResponse> 
        where TResponse : IResponse 
    {
        Task<TResponse> Handle(TRequest request);
    }
}
