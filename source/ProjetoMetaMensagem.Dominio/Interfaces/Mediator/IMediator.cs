using ProjetoMetaMensagem.Dominio.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjetoMetaMensagem.Dominio.Interfaces.Mediator
{
    public interface IMediator
    {
        // Envia uma requisição que espera um retorno específico.
        // TResponse DEVE ser um tipo que implemente IResponse.
        Task<TResponse> Send<TResponse>(IRequest<TResponse> request) where TResponse : IResponse;


    }
}
