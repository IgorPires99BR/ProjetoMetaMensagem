using ProjetoMetaMensagem.Dominio.Common;
using ProjetoMetaMensagem.Dominio.Interfaces.Mediator;
using ProjetoMetaMensagem.Dominio.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ProjetoMetaMensagem.Dominio.Help.Error;

namespace ProjetoMetaMensagem.Dominio.UseCases.Contato.DeletaContato
{
    public class DeletaContatoHandler : IRequestHandler<DeletaContatoCommand, Response<DeletaContatoResult>>
    {
        private readonly IUnitOfWork _unitOfWork;

        public DeletaContatoHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<Response<DeletaContatoResult>> Handle(DeletaContatoCommand command)
        {
            var response = new Response<DeletaContatoResult>();

            var validator = new DeletaContatoValidator();
            var validateResult = validator.Validate(command);

            if (!validateResult.IsValid)
            {
                response.AddErros(validateResult.Errors.ToCustomValidationFailure());
                return response;
            }

            // Lógica para remover o contato via UnitOfWork aqui
            await _unitOfWork.Contato.Excluir(command.Id);


            response.AddValue(new DeletaContatoResult());
            return response;
        }
    }
}
