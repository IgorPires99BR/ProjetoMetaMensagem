using Microsoft.Extensions.Logging;
using ProjetoMetaMensagem.Dominio.Common;
using ProjetoMetaMensagem.Dominio.Help.Error;
using ProjetoMetaMensagem.Dominio.Interfaces;
using ProjetoMetaMensagem.Dominio.Interfaces.Mediator;
using System;
using System.Threading.Tasks;

namespace ProjetoMetaMensagem.Dominio.UseCases.Webhook.DeletaWebhook
{
    public class DeletaWebhookHandler : IRequestHandler<DeletaWebhookCommand, Response<DeletaWebhookResult>>
    {
        private readonly IUnitOfWork _unitOfWork;

        private readonly ILogger<DeletaWebhookHandler> _logger;

        public DeletaWebhookHandler(IUnitOfWork unitOfWork, ILogger<DeletaWebhookHandler> logger)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async Task<Response<DeletaWebhookResult>> Handle(DeletaWebhookCommand command)
        {
            var response = new Response<DeletaWebhookResult>();

            try
            {
                _unitOfWork.BeginTransaction();
                var validator = new DeletaWebhookValidator();
                var validateResult = validator.Validate(command);

                if (!validateResult.IsValid)
                {
                    response.AddErros(validateResult.Errors.ToCustomValidationFailure());
                    return response;
                }

                var linhasAfetadas = await _unitOfWork.WebhookConfig.Excluir(
                    command.Id, command.EmpresaIdSolicitante);

                // Zero linhas significa que o webhook nao existe OU pertence a outra empresa.
                // As duas situacoes devolvem a mesma mensagem de proposito: dizer "existe, mas
                // nao e seu" ja entregaria ao atacante que aquele id e valido.
                if (linhasAfetadas == 0)
                {
                    _unitOfWork.Rollback();
                    response.AddErro("Webhook não encontrado.");
                    return response;
                }

                response.AddValue(new DeletaWebhookResult());
                _unitOfWork.Commit();
            }
            catch (Exception ex)
            {
                    _unitOfWork.Rollback();
                
                response.AddErroServico(ex, _logger, nameof(DeletaWebhookHandler));
            }

            return response;
        }
    }
}


