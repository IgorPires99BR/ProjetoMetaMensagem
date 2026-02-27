using ProjetoMetaMensagem.Dominio.Interfaces.Mediator;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjetoMetaMensagem.Dominio.Common
{
    public class Mediator : IMediator
    {
        private readonly IServiceProvider _serviceProvider;

        public Mediator(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        // Implementação para IRequest<TResponse>
        // A RESTRIÇÃO 'where TResponse : IResponse' AGORA CORRESPONDE À DA INTERFACE
        public async Task<TResponse> Send<TResponse>(IRequest<TResponse> request) where TResponse : IResponse
        {
            // Resolve o handler específico para a requisição e tipo de resposta
            var handlerType = typeof(IRequestHandler<,>).MakeGenericType(request.GetType(), typeof(TResponse));
            dynamic handler = _serviceProvider.GetRequiredService(handlerType);
            return await handler.Handle((dynamic)request);
        }
    }
}
