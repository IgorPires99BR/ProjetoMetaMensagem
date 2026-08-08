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

namespace ProjetoMetaMensagem.Dominio.UseCases.Contato.AlteraContato
{
    public class AlteraContatoHandler : IRequestHandler<AlteraContatoCommand, Response<AlteraContatoResult>>
    {
        private readonly IUnitOfWork _unitOfWork;

        private readonly ILogger<AlteraContatoHandler> _logger;

        public AlteraContatoHandler(IUnitOfWork unitOfWork, ILogger<AlteraContatoHandler> logger)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async Task<Response<AlteraContatoResult>> Handle(AlteraContatoCommand command)
        {
            var response = new Response<AlteraContatoResult>();

            try
            {
                _unitOfWork.BeginTransaction();
                var validator = new AlteraContatoValidator();
                var validateResult = validator.Validate(command);

                if (!validateResult.IsValid)
                {
                    response.AddErros(validateResult.Errors.ToCustomValidationFailure());
                    return response;
                }

                var linhasAfetadas = await _unitOfWork.Contato.Alterar(
                    new Entidades.Contato(command), command.EmpresaIdSolicitante);

                // Zero linhas: contato inexistente ou de outra empresa. Mesma mensagem nos dois
                // casos, pra nao confirmar ao atacante que o id existe.
                if (linhasAfetadas == 0)
                {
                    _unitOfWork.Rollback();
                    response.AddErro("Contato não encontrado.");
                    return response;
                }

                response.AddValue(new AlteraContatoResult());
                _unitOfWork.Commit();
            }
            catch (Exception ex)
            {
                    _unitOfWork.Rollback();
                
                response.AddErro(TratamentoErro.Tratar(ex, _logger, nameof(AlteraContatoHandler)));
            }

            return response;
        }
    }
}


