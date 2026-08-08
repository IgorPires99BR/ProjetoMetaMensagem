using Microsoft.Extensions.Logging;
using ProjetoMetaMensagem.Dominio.Common;
using ProjetoMetaMensagem.Dominio.Help.Error;
using ProjetoMetaMensagem.Dominio.Interfaces;
using ProjetoMetaMensagem.Dominio.Interfaces.Mediator;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ProjetoMetaMensagem.Dominio.UseCases.Webhook.ListaWebhook
{
    public class ListaWebhookHandler : IRequestHandler<ListaWebhookCommand, Response<List<ListaWebhookResult>>>
    {
        private readonly IUnitOfWork _unitOfWork;

        private readonly ILogger<ListaWebhookHandler> _logger;

        public ListaWebhookHandler(IUnitOfWork unitOfWork, ILogger<ListaWebhookHandler> logger)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async Task<Response<List<ListaWebhookResult>>> Handle(ListaWebhookCommand command)
        {
            var response = new Response<List<ListaWebhookResult>>();

            try
            {
                var listaWebhooks = new List<ListaWebhookResult>();

                var webhooks = await _unitOfWork.WebhookConfig.ObterPorEmpresa(command.EmpresaId);

                foreach (var webhook in webhooks)
                {
                    listaWebhooks.Add(new ListaWebhookResult(webhook));
                }

                response.AddValue(listaWebhooks);
            }
            catch (Exception ex)
            {
                response.AddErro(TratamentoErro.Tratar(ex, _logger, nameof(ListaWebhookHandler)));
            }

            return response;
        }
    }
}
