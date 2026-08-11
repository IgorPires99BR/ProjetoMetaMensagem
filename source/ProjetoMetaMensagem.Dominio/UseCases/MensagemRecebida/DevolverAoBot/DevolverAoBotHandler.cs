using ProjetoMetaMensagem.Dominio.Help.Error;
using Microsoft.Extensions.Logging;
using ProjetoMetaMensagem.Dominio.Common;
using ProjetoMetaMensagem.Dominio.Interfaces;
using ProjetoMetaMensagem.Dominio.Interfaces.Mediator;
using System;
using System.Threading.Tasks;

namespace ProjetoMetaMensagem.Dominio.UseCases.MensagemRecebida.DevolverAoBot
{
    public class DevolverAoBotHandler : IRequestHandler<DevolverAoBotCommand, Response<DevolverAoBotResult>>
    {
        private readonly IUnitOfWork _unitOfWork;

        private readonly ILogger<DevolverAoBotHandler> _logger;

        public DevolverAoBotHandler(IUnitOfWork unitOfWork, ILogger<DevolverAoBotHandler> logger)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async Task<Response<DevolverAoBotResult>> Handle(DevolverAoBotCommand command)
        {
            var response = new Response<DevolverAoBotResult>();

            var validator = new DevolverAoBotValidator();
            var validateResult = validator.Validate(command);
            if (!validateResult.IsValid)
            {
                response.AddErros(validateResult.Errors.ToCustomValidationFailure());
                return response;
            }

            try
            {
                var estadoAtivo = await _unitOfWork.ConversationState.ObterPorEmpresaEContato(command.EmpresaId, command.ContatoId);
                if (estadoAtivo != null && estadoAtivo.AssumidoPorUsuarioId != null)
                {
                    await _unitOfWork.ConversationState.DevolverAoBot(estadoAtivo.Id);
                }

                response.AddValue(new DevolverAoBotResult());
            }
            catch (Exception ex)
            {
                response.AddErroServico(ex, _logger, nameof(DevolverAoBotHandler));
            }

            return response;
        }
    }
}
