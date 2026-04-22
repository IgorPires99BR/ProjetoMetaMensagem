using ProjetoMetaMensagem.Dominio.Common;
using ProjetoMetaMensagem.Dominio.Interfaces.Mediator;
using ProjetoMetaMensagem.Dominio.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ProjetoMetaMensagem.Dominio.Help.Error;

namespace ProjetoMetaMensagem.Dominio.UseCases.Numero.CriaNumero
{
    public class CriaNumeroHandler : IRequestHandler<CriaNumeroCommand, Response<CriaNumeroResult>>
    {
        private readonly IUnitOfWork _unitOfWork;

        public CriaNumeroHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<Response<CriaNumeroResult>> Handle(CriaNumeroCommand command)
        {
            var response = new Response<CriaNumeroResult>();

            var validator = new CriaNumeroValidator();
            var validateResult = validator.Validate(command);

            if (!validateResult.IsValid)
            {
                response.AddErros(validateResult.Errors.ToCustomValidationFailure());
                return response;
            }

            // Lógica de persistência (Exemplo)
            // var novoNumero = new EntidadeNumero(command.Propriedade);
            await _unitOfWork.Numero.Incluir(new Entidades.Numero(command));
            // await _unitOfWork.Commit();

            response.AddValue(new CriaNumeroResult());

            return response;
        }
    }
}
