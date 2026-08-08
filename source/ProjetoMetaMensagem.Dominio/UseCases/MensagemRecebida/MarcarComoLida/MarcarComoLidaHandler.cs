using ProjetoMetaMensagem.Dominio.Help.Error;
using Microsoft.Extensions.Logging;
using ProjetoMetaMensagem.Dominio.Common;
using ProjetoMetaMensagem.Dominio.Interfaces;
using ProjetoMetaMensagem.Dominio.Interfaces.Mediator;
using System;
using System.Threading.Tasks;

namespace ProjetoMetaMensagem.Dominio.UseCases.MensagemRecebida.MarcarComoLida
{
    public class MarcarComoLidaHandler : IRequestHandler<MarcarComoLidaCommand, Response<MarcarComoLidaResult>>
    {
        private readonly IUnitOfWork _unitOfWork;

        private readonly ILogger<MarcarComoLidaHandler> _logger;

        public MarcarComoLidaHandler(IUnitOfWork unitOfWork, ILogger<MarcarComoLidaHandler> logger)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async Task<Response<MarcarComoLidaResult>> Handle(MarcarComoLidaCommand command)
        {
            var response = new Response<MarcarComoLidaResult>();

            var validator = new MarcarComoLidaValidator();
            var validateResult = validator.Validate(command);
            if (!validateResult.IsValid)
            {
                response.AddErros(validateResult.Errors.ToCustomValidationFailure());
                return response;
            }

            try
            {
                await _unitOfWork.MensagemRecebida.MarcarTodasComoLidas(command.EmpresaId, command.ContatoId);
                //_unitOfWork.Commit();

                response.AddValue(new MarcarComoLidaResult());
            }
            catch (Exception ex)
            {
                response.AddErroServico(ex, _logger, nameof(MarcarComoLidaHandler));
            }

            return response;
        }
    }
}
