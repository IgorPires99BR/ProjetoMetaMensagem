using ProjetoMetaMensagem.Dominio.Common;
using ProjetoMetaMensagem.Dominio.Help.Error;
using ProjetoMetaMensagem.Dominio.Interfaces;
using ProjetoMetaMensagem.Dominio.Interfaces.Mediator;
using ProjetoMetaMensagem.Dominio.UseCases.Empresa.AlteraEmpresa;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjetoMetaMensagem.Dominio.UseCases.Empresa.DeletaEmpresa
{
    public class DeletaEmpresaHandler : IRequestHandler<DeletaEmpresaCommand, Response<DeletaEmpresaResult>>
    {
        private readonly IUnitOfWork _unitOfWork;

        public DeletaEmpresaHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }


        public async Task<Response<DeletaEmpresaResult>> Handle(DeletaEmpresaCommand command)
        {
            var response = new Response<DeletaEmpresaResult>();

            try
            {
                _unitOfWork.BeginTransaction();
                // Validação: criar e usar um validador específico (AlteraEmpresaValidator) similar ao CriaEmpresaValidator
                var validator = new DeletaEmpresaValidator();
                var validateResult = validator.Validate(command);

                if (!validateResult.IsValid)
                {
                    response.AddErros(validateResult.Errors.ToCustomValidationFailure());
                    return response;
                }

                await _unitOfWork.Empresa.Excluir(command.IdEmpresa);
                _unitOfWork.Commit();
            }
            catch (Exception ex)
            {
                    _unitOfWork.Rollback();
                
                response.AddErro($"Erro: {ex.Message}");
            }

            return response;
        }
    }
}


