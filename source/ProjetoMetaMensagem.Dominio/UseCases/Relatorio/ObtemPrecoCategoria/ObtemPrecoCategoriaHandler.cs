using Microsoft.Extensions.Logging;
using ProjetoMetaMensagem.Dominio.Common;
using ProjetoMetaMensagem.Dominio.Interfaces;
using ProjetoMetaMensagem.Dominio.Interfaces.Mediator;
using System;
using System.Threading.Tasks;

namespace ProjetoMetaMensagem.Dominio.UseCases.Relatorio.ObtemPrecoCategoria
{
    public class ObtemPrecoCategoriaHandler : IRequestHandler<ObtemPrecoCategoriaCommand, Response<ObtemPrecoCategoriaResult>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<ObtemPrecoCategoriaHandler> _logger;

        public ObtemPrecoCategoriaHandler(IUnitOfWork unitOfWork, ILogger<ObtemPrecoCategoriaHandler> logger)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async Task<Response<ObtemPrecoCategoriaResult>> Handle(ObtemPrecoCategoriaCommand command)
        {
            var response = new Response<ObtemPrecoCategoriaResult>();

            if (!command.SolicitanteEhAdmin)
            {
                response.AddErro("Apenas administradores podem ver o preço por categoria.");
                return response;
            }

            try
            {
                var precos = await _unitOfWork.Relatorio.ListarPrecosCategoria();
                response.AddValue(new ObtemPrecoCategoriaResult { Precos = precos });
            }
            catch (Exception ex)
            {
                response.AddErroServico(ex, _logger, nameof(ObtemPrecoCategoriaHandler));
            }

            return response;
        }
    }
}
