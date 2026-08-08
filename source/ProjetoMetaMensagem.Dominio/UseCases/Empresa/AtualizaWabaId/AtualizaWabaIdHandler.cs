using Microsoft.Extensions.Logging;
using ProjetoMetaMensagem.Dominio.Common;
using ProjetoMetaMensagem.Dominio.Help.Error;
using ProjetoMetaMensagem.Dominio.Interfaces;
using ProjetoMetaMensagem.Dominio.Interfaces.Mediator;
using ProjetoMetaMensagem.Dominio.Interfaces.Servicos;
using ProjetoMetaMensagem.Dominio.UseCases.Empresa.CriaEmpresa;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjetoMetaMensagem.Dominio.UseCases.Empresa.AtualizaWabaId
{
    public class AtualizaWabaIdHandler : IRequestHandler<AtualizaWabaIdCommand, Response<AtualizaWabaIdResult>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMetaService _metaService;
        private readonly ILogger<AtualizaWabaIdHandler> _logger;

        public AtualizaWabaIdHandler(IUnitOfWork unitOfWork, IMetaService metaService, ILogger<AtualizaWabaIdHandler> logger)
        {
            _unitOfWork = unitOfWork;
            _metaService = metaService;
            _logger = logger;
        }

        public async Task<Response<AtualizaWabaIdResult>> Handle(AtualizaWabaIdCommand command)
        {
            var response = new Response<AtualizaWabaIdResult>();

            var validator = new AtualizaWabaIdValidator();
            var validateResult = validator.Validate(command);
            if (!validateResult.IsValid)
            {
                response.AddErros(validateResult.Errors.ToCustomValidationFailure());
                return response;
            }

            try
            {
                _unitOfWork.BeginTransaction();

                if (command.AccessToken != null & !String.IsNullOrEmpty(command.AccessToken))
                {
                    var wabaID = await _metaService.BuscarWabaIdDaMetaAsync(command.AccessToken);

                    await _unitOfWork.Empresa.AtualizarWabaId(command.IdEmpresa, wabaID);
                }

                response.AddValue(new AtualizaWabaIdResult());
                _unitOfWork.Commit();
            }
            catch (Exception ex)
            {
                _unitOfWork.Rollback();
                response.AddErroServico(ex, _logger, nameof(AtualizaWabaIdHandler));
            }

            return response;
        }
    }
}
