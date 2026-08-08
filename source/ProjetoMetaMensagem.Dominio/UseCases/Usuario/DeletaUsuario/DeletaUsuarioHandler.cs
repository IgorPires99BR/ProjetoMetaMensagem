using Microsoft.Extensions.Logging;
using ProjetoMetaMensagem.Dominio.Common;
using ProjetoMetaMensagem.Dominio.Help.Error;
using ProjetoMetaMensagem.Dominio.Interfaces;
using ProjetoMetaMensagem.Dominio.Interfaces.Mediator;
using ProjetoMetaMensagem.Dominio.UseCases.Usuario.AlteraUsuario;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjetoMetaMensagem.Dominio.UseCases.Usuario.DeletaUsuario
{
    public class DeletaUsuarioHandler : IRequestHandler<DeletaUsuarioCommand, Response<DeletaUsuarioResult>>
    {
        private readonly IUnitOfWork _unitOfWork;

        private readonly ILogger<DeletaUsuarioHandler> _logger;

        public DeletaUsuarioHandler(IUnitOfWork unitOfWork, ILogger<DeletaUsuarioHandler> logger)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async Task<Response<DeletaUsuarioResult>> Handle(DeletaUsuarioCommand command)
        {
            var response = new Response<DeletaUsuarioResult>();

            try
            {
                _unitOfWork.BeginTransaction();
                var validator = new DeletaUsuarioValidator();
                var validateResult = validator.Validate(command);

                if (!validateResult.IsValid)
                {
                    response.AddErros(validateResult.Errors.ToCustomValidationFailure());
                    return response;
                }

                var linhasAfetadas = await _unitOfWork.Usuario.Excluir(
                    command.Id.ToString(), command.EmpresaIdSolicitante);

                // Zero linhas significa que o usuario nao existe OU pertence a outra empresa.
                // As duas situacoes devolvem a mesma mensagem de proposito: dizer "existe, mas
                // nao e seu" ja entregaria ao atacante que aquele id e valido.
                if (linhasAfetadas == 0)
                {
                    _unitOfWork.Rollback();
                    response.AddErro("Usuário não encontrado.");
                    return response;
                }

                response.AddValue(new DeletaUsuarioResult());
                _unitOfWork.Commit();
            }
            catch (Exception ex)
            {
                    _unitOfWork.Rollback();
                
                response.AddErro(TratamentoErro.Tratar(ex, _logger, nameof(DeletaUsuarioHandler)));
            }

            return response;
        }
    }
}


