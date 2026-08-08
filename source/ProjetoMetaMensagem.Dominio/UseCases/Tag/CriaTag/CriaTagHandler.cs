using Microsoft.Extensions.Logging;
using ProjetoMetaMensagem.Dominio.Common;
using ProjetoMetaMensagem.Dominio.Interfaces.Mediator;
using ProjetoMetaMensagem.Dominio.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ProjetoMetaMensagem.Dominio.Help.Error;

namespace ProjetoMetaMensagem.Dominio.UseCases.Tag.CriaTag
{
    public class CriaTagHandler : IRequestHandler<CriaTagCommand, Response<CriaTagResult>>
    {
        private readonly IUnitOfWork _unitOfWork;

        private readonly ILogger<CriaTagHandler> _logger;

        public CriaTagHandler(IUnitOfWork unitOfWork, ILogger<CriaTagHandler> logger)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async Task<Response<CriaTagResult>> Handle(CriaTagCommand command)
        {
            var response = new Response<CriaTagResult>();

            try
            {
                _unitOfWork.BeginTransaction();
                var validator = new CriaTagValidator();
                var validateResult = validator.Validate(command);

                if (!validateResult.IsValid)
                {
                    response.AddErros(validateResult.Errors.ToCustomValidationFailure());
                    return response;
                }

                var entity = new Entidades.Tag(command);
                await _unitOfWork.Tag.Incluir(entity);

                response.AddValue(new CriaTagResult { Id = entity.Id });
                _unitOfWork.Commit();
            }
            catch (Exception ex)
            {
                    _unitOfWork.Rollback();
                
                response.AddErroServico(ex, _logger, nameof(CriaTagHandler));
            }

            return response;
        }
    }
}


