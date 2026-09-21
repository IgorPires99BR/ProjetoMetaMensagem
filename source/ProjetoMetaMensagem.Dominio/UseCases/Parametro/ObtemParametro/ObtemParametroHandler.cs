using Microsoft.Extensions.Logging;
using ProjetoMetaMensagem.Dominio.Common;
using ProjetoMetaMensagem.Dominio.Help.Error;
using ProjetoMetaMensagem.Dominio.Interfaces;
using ProjetoMetaMensagem.Dominio.Interfaces.Mediator;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ProjetoMetaMensagem.Dominio.UseCases.Parametro.ObtemParametro
{
    public class ObtemParametroHandler : IRequestHandler<ObtemParametroCommand, Response<List<ObtemParametroResult>>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<ObtemParametroHandler> _logger;

        public ObtemParametroHandler(IUnitOfWork unitOfWork, ILogger<ObtemParametroHandler> logger)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async Task<Response<List<ObtemParametroResult>>> Handle(ObtemParametroCommand command)
        {
            var response = new Response<List<ObtemParametroResult>>();

            try
            {
                var parametros = await _unitOfWork.Parametro.ObterPorEmpresa(command.EmpresaId);
                response.AddValue(parametros.Select(p => new ObtemParametroResult(p)).ToList());
            }
            catch (Exception ex)
            {
                response.AddErroServico(ex, _logger, nameof(ObtemParametroHandler));
            }

            return response;
        }
    }
}
