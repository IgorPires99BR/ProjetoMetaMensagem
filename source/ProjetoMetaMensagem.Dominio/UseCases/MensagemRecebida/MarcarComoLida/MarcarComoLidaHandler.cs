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

            try
            {
                await _unitOfWork.MensagemRecebida.MarcarTodasComoLidas(command.EmpresaId, command.ContatoId);
                //_unitOfWork.Commit();

                response.AddValue(new MarcarComoLidaResult());
            }
            catch (Exception ex)
            {
                response.AddErro(TratamentoErro.Tratar(ex, _logger, nameof(MarcarComoLidaHandler)));
            }

            return response;
        }
    }
}
