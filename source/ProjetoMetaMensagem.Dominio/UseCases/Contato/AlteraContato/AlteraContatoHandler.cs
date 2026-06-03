using ProjetoMetaMensagem.Dominio.Common;
using ProjetoMetaMensagem.Dominio.Interfaces.Mediator;
using ProjetoMetaMensagem.Dominio.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ProjetoMetaMensagem.Dominio.Help.Error;

namespace ProjetoMetaMensagem.Dominio.UseCases.Contato.AlteraContato
{
    public class AlteraContatoHandler : IRequestHandler<AlteraContatoCommand, Response<AlteraContatoResult>>
    {
        private readonly IUnitOfWork _unitOfWork;

        public AlteraContatoHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<Response<AlteraContatoResult>> Handle(AlteraContatoCommand command)
        {
            var response = new Response<AlteraContatoResult>();

            var validator = new AlteraContatoValidator();
            var validateResult = validator.Validate(command);

            if (!validateResult.IsValid)
            {
                response.AddErros(validateResult.Errors.ToCustomValidationFailure());
                return response;
            }

            // Lógica para atualizar o contato via UnitOfWork aqui
            await _unitOfWork.Contato.Alterar(new Entidades.Contato(command));


            response.AddValue(new AlteraContatoResult());
            return response;
        }
    }
}
