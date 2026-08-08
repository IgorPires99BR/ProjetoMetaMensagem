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

namespace ProjetoMetaMensagem.Dominio.UseCases.Numero.DeletaNumero
{
    public class DeletaNumeroHandler : IRequestHandler<DeletaNumeroCommand, Response<DeletaNumeroResult>>
    {
        private readonly IUnitOfWork _unitOfWork;

        private readonly ILogger<DeletaNumeroHandler> _logger;

        public DeletaNumeroHandler(IUnitOfWork unitOfWork, ILogger<DeletaNumeroHandler> logger)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async Task<Response<DeletaNumeroResult>> Handle(DeletaNumeroCommand command)
        {
            var response = new Response<DeletaNumeroResult>();

            try
            {
                _unitOfWork.BeginTransaction();
                var validator = new DeletaNumeroValidator();
                var validateResult = validator.Validate(command);

                if (!validateResult.IsValid)
                {
                    response.AddErros(validateResult.Errors.ToCustomValidationFailure());
                    return response;
                }

                var linhasAfetadas = await _unitOfWork.Numero.Excluir(command.Id, command.EmpresaIdSolicitante);

                // Zero linhas significa que o numero nao existe OU pertence a outra empresa.
                // As duas situacoes devolvem a mesma mensagem de proposito: dizer "existe, mas
                // nao e seu" ja entregaria ao atacante que aquele id e valido.
                if (linhasAfetadas == 0)
                {
                    _unitOfWork.Rollback();
                    response.AddErro("Número não encontrado.");
                    return response;
                }

                response.AddValue(new DeletaNumeroResult());
                _unitOfWork.Commit();
            }
            catch (Exception ex)
            {
                    _unitOfWork.Rollback();
                
                response.AddErro(TratamentoErro.Tratar(ex, _logger, nameof(DeletaNumeroHandler)));
            }

            return response;
        }
    }
}


