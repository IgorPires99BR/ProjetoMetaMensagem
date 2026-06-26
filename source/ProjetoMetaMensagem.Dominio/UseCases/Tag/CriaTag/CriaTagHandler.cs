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

        public CriaTagHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<Response<CriaTagResult>> Handle(CriaTagCommand command)
        {
            var response = new Response<CriaTagResult>();

            try
            {
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
            }
            catch (Exception ex)
            {
                response.AddErro($"Erro: {ex.Message}");
            }

            return response;
        }
    }
}
