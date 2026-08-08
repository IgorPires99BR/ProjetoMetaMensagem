using Microsoft.Extensions.Logging;
using ProjetoMetaMensagem.Dominio.Common;
using ProjetoMetaMensagem.Dominio.Entidades;
using ProjetoMetaMensagem.Dominio.Help.Error;
using ProjetoMetaMensagem.Dominio.Interfaces;
using ProjetoMetaMensagem.Dominio.Interfaces.Mediator;
using System;
using System.Threading.Tasks;

namespace ProjetoMetaMensagem.Dominio.UseCases.Webhook.CriaWebhook
{
    public class CriaWebhookHandler : IRequestHandler<CriaWebhookCommand, Response<CriaWebhookResult>>
    {
        private readonly IUnitOfWork _unitOfWork;

        private readonly ILogger<CriaWebhookHandler> _logger;

        public CriaWebhookHandler(IUnitOfWork unitOfWork, ILogger<CriaWebhookHandler> logger)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async Task<Response<CriaWebhookResult>> Handle(CriaWebhookCommand command)
        {
            var response = new Response<CriaWebhookResult>();

            try
            {
                _unitOfWork.BeginTransaction();
                var validator = new CriaWebhookValidator();
                var validateResult = validator.Validate(command);

                if (!validateResult.IsValid)
                {
                    response.AddErros(validateResult.Errors.ToCustomValidationFailure());
                    return response;
                }

                var webhookConfig = new WebhookConfig
                {
                    EmpresaId = command.EmpresaId,
                    Nome = command.Nome,
                    Url = command.Url,
                    Evento = command.Evento
                };
                var id = await _unitOfWork.WebhookConfig.Incluir(webhookConfig);

                response.AddValue(new CriaWebhookResult { Id = id });
                _unitOfWork.Commit();
            }
            catch (Exception ex)
            {
                    _unitOfWork.Rollback();
                
                response.AddErro(TratamentoErro.Tratar(ex, _logger, nameof(CriaWebhookHandler)));
            }

            return response;
        }
    }
}


