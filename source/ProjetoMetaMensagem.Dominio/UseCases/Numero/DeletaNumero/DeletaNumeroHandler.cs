using ProjetoMetaMensagem.Dominio.Common;
using ProjetoMetaMensagem.Dominio.Interfaces.Mediator;
using ProjetoMetaMensagem.Dominio.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ProjetoMetaMensagem.Dominio.Help.Error;

namespace ProjetoMetaMensagem.Dominio.UseCases.Numero.DeletaNumero
{
    public class DeletaNumeroHandler : IRequestHandler<DeletaNumeroCommand, Response<DeletaNumeroResult>>
    {
        private readonly IUnitOfWork _unitOfWork;

        public DeletaNumeroHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<Response<DeletaNumeroResult>> Handle(DeletaNumeroCommand command)
        {
            var response = new Response<DeletaNumeroResult>();

            var validator = new DeletaNumeroValidator();
            var validateResult = validator.Validate(command);

            if (!validateResult.IsValid)
            {
                response.AddErros(validateResult.Errors.ToCustomValidationFailure());
                return response;
            }

            // Lógica de exclusão (Exemplo)
             await _unitOfWork.Numero.Excluir(command.Id);

            response.AddValue(new DeletaNumeroResult());

            return response;
        }
    }
}
