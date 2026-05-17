using ProjetoMetaMensagem.Dominio.Common;
using ProjetoMetaMensagem.Dominio.Interfaces.Mediator;
using ProjetoMetaMensagem.Dominio.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ProjetoMetaMensagem.Dominio.Help.Error;

namespace ProjetoMetaMensagem.Dominio.UseCases.Contato.CriaContato
{
    public class CriaContatoHandler : IRequestHandler<CriaContatoCommand, Response<CriaContatoResult>>
    {
        private readonly IUnitOfWork _unitOfWork;

        public CriaContatoHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<Response<CriaContatoResult>> Handle(CriaContatoCommand command)
        {
            var response = new Response<CriaContatoResult>();

            var validator = new CriaContatoValidator();
            var validateResult = validator.Validate(command);

            if (!validateResult.IsValid)
            {
                response.AddErros(validateResult.Errors.ToCustomValidationFailure());
                return response;
            }

            // Lógica para adicionar o contato via UnitOfWork aqui
            await _unitOfWork.Contato.Incluir(new Entidades.Contato(command));

            response.AddValue(new CriaContatoResult());
            return response;
        }
    }
}
