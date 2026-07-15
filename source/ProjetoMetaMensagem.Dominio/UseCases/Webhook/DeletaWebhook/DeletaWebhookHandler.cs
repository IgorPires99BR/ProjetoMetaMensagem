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

        public DeletaWebhookHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
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

                await _unitOfWork.WebhookConfig.Excluir(command.Id);

                response.AddValue(new DeletaWebhookResult());
                _unitOfWork.Commit();
            }
            catch (Exception ex)
            {
                    _unitOfWork.Rollback();
                
                response.AddErro($"Erro: {ex.Message}");
            }

            return response;
        }
    }
}


