using Microsoft.Extensions.Logging;
using ProjetoMetaMensagem.Dominio.Common;
using ProjetoMetaMensagem.Dominio.Help.Error;
using ProjetoMetaMensagem.Dominio.Interfaces;
using ProjetoMetaMensagem.Dominio.Interfaces.Mediator;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ProjetoMetaMensagem.Dominio.UseCases.Perfil.ObtemPerfil
{
    public class ObtemPerfilHandler : IRequestHandler<ObtemPerfilCommand, Response<List<ObtemPerfilResult>>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<ObtemPerfilHandler> _logger;

        public ObtemPerfilHandler(IUnitOfWork unitOfWork, ILogger<ObtemPerfilHandler> logger)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async Task<Response<List<ObtemPerfilResult>>> Handle(ObtemPerfilCommand command)
        {
            var response = new Response<List<ObtemPerfilResult>>();

            try
            {
                var perfis = await _unitOfWork.Perfil.ObterPorEmpresa(command.EmpresaIdSolicitante);
                response.AddValue(perfis.Select(p => new ObtemPerfilResult(p)).ToList());
            }
            catch (Exception ex)
            {
                response.AddErroServico(ex, _logger, nameof(ObtemPerfilHandler));
            }

            return response;
        }
    }
}
