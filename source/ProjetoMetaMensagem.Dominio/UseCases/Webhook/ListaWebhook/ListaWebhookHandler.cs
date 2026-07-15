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

        public ListaWebhookHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
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
                response.AddErro($"Erro: {ex.Message}");
            }

            return response;
        }
    }
}
