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

namespace ProjetoMetaMensagem.Dominio.UseCases.Contato.DeletaContato
{
    public class DeletaContatoHandler : IRequestHandler<DeletaContatoCommand, Response<DeletaContatoResult>>
    {
        private readonly IUnitOfWork _unitOfWork;

        private readonly ILogger<DeletaContatoHandler> _logger;

        public DeletaContatoHandler(IUnitOfWork unitOfWork, ILogger<DeletaContatoHandler> logger)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async Task<Response<DeletaContatoResult>> Handle(DeletaContatoCommand command)
        {
            var response = new Response<DeletaContatoResult>();

            try
            {
                _unitOfWork.BeginTransaction();
                var validator = new DeletaContatoValidator();
                var validateResult = validator.Validate(command);

                if (!validateResult.IsValid)
                {
                    response.AddErros(validateResult.Errors.ToCustomValidationFailure());
                    return response;
                }

                var linhasAfetadas = await _unitOfWork.Contato.Excluir(command.Id, command.EmpresaIdSolicitante);

                // Zero linhas significa que o contato nao existe OU pertence a outra empresa.
                // As duas situacoes devolvem a mesma mensagem de proposito: dizer "existe, mas
                // nao e seu" ja entregaria ao atacante que aquele id e valido.
                if (linhasAfetadas == 0)
                {
                    _unitOfWork.Rollback();
                    response.AddErro("Contato não encontrado.");
                    return response;
                }

                response.AddValue(new DeletaContatoResult());
                _unitOfWork.Commit();
            }
            catch (Exception ex)
            {
                    _unitOfWork.Rollback();
                
                response.AddErroServico(ex, _logger, nameof(DeletaContatoHandler));
            }

            return response;
        }
    }
}


