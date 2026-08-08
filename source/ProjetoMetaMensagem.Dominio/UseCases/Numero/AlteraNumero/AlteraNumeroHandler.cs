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

namespace ProjetoMetaMensagem.Dominio.UseCases.Numero.AlteraNumero
{
    public class AlteraNumeroHandler : IRequestHandler<AlteraNumeroCommand, Response<AlteraNumeroResult>>
    {
        private readonly IUnitOfWork _unitOfWork;

        private readonly ILogger<AlteraNumeroHandler> _logger;

        public AlteraNumeroHandler(IUnitOfWork unitOfWork, ILogger<AlteraNumeroHandler> logger)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
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

                var linhasAfetadas = await _unitOfWork.Numero.Alterar(
                    new Entidades.Numero(command), command.EmpresaIdSolicitante);

                // Zero linhas: numero inexistente ou de outra empresa. Mesma mensagem nos dois
                // casos, pra nao confirmar ao atacante que o id existe.
                if (linhasAfetadas == 0)
                {
                    _unitOfWork.Rollback();
                    response.AddErro("Número não encontrado.");
                    return response;
                }

                response.AddValue(new AlteraNumeroResult());
                _unitOfWork.Commit();
            }
            catch (Exception ex)
            {
                    _unitOfWork.Rollback();
                
                response.AddErroServico(ex, _logger, nameof(AlteraNumeroHandler));
            }

            return response;
        }
    }
}


