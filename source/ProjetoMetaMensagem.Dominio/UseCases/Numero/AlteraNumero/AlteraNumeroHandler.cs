using ProjetoMetaMensagem.Dominio.Common;
using ProjetoMetaMensagem.Dominio.Interfaces.Mediator;
using ProjetoMetaMensagem.Dominio.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ProjetoMetaMensagem.Dominio.Help.Error;

namespace ProjetoMetaMensagem.Dominio.UseCases.Numero.AlteraNumero
{
    public class AlteraNumeroHandler : IRequestHandler<AlteraNumeroCommand, Response<AlteraNumeroResult>>
    {
        private readonly IUnitOfWork _unitOfWork;

        public AlteraNumeroHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<Response<AlteraNumeroResult>> Handle(AlteraNumeroCommand command)
        {
            var response = new Response<AlteraNumeroResult>();

            try
            {
                _unitOfWork.BeginTransaction();
                var validator = new AlteraNumeroValidator();
                var validateResult = validator.Validate(command);

                if (!validateResult.IsValid)
                {
                    response.AddErros(validateResult.Errors.ToCustomValidationFailure());
                    return response;
                }

                // Lógica de alteração (Exemplo)
                await _unitOfWork.Numero.Alterar(new Entidades.Numero(command));

                response.AddValue(new AlteraNumeroResult());
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


