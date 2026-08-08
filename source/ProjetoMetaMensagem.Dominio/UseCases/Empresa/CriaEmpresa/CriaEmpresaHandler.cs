using Microsoft.Extensions.Logging;
using ProjetoMetaMensagem.Dominio.Common;
using ProjetoMetaMensagem.Dominio.Help.Error;
using ProjetoMetaMensagem.Dominio.Interfaces;
using ProjetoMetaMensagem.Dominio.Interfaces.Mediator;
using ProjetoMetaMensagem.Dominio.Interfaces.Servicos;
using ProjetoMetaMensagem.Dominio.UseCases.Flows.CriaFlow;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjetoMetaMensagem.Dominio.UseCases.Empresa.CriaEmpresa
{
    public class CriaEmpresaHandler : IRequestHandler<CriaEmpresaCommand, Response<CriaEmpresaResult>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMetaService _metaService;
        private readonly ILogger<CriaEmpresaHandler> _logger;

        public CriaEmpresaHandler(IUnitOfWork unitOfWork, IMetaService metaService, ILogger<CriaEmpresaHandler> logger)
        {
            _unitOfWork = unitOfWork;
            _metaService = metaService;
            _logger = logger;
        }

        public async Task<Response<CriaEmpresaResult>> Handle(CriaEmpresaCommand command)
        {
            var response = new Response<CriaEmpresaResult>();

            try
            {
                _unitOfWork.BeginTransaction();
                var validator = new CriaEmpresaValidator();
                var validateResult = validator.Validate(command);

                if (!validateResult.IsValid)
                {
                    response.AddErros(validateResult.Errors.ToCustomValidationFailure());
                    return response;
                }

                var resultado = await _unitOfWork.Empresa.Incluir(new Entidades.Empresa(command));

                response.AddValue(new CriaEmpresaResult());
                _unitOfWork.Commit();
            }
            catch (Exception ex)
            {
                    _unitOfWork.Rollback();
                
                response.AddErroServico(ex, _logger, nameof(CriaEmpresaHandler));
            }

            return response;
        }
    }
}


