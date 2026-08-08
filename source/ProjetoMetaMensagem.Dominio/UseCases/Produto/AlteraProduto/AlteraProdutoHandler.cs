using Microsoft.Extensions.Logging;
using ProjetoMetaMensagem.Dominio.Common;
using ProjetoMetaMensagem.Dominio.Interfaces;
using ProjetoMetaMensagem.Dominio.Interfaces.Mediator;
using ProjetoMetaMensagem.Dominio.Help.Error;
using System.Threading.Tasks;

namespace ProjetoMetaMensagem.Dominio.UseCases.Produto.AlteraProduto
{
    public class AlteraProdutoHandler : IRequestHandler<AlteraProdutoCommand, Response<AlteraProdutoResult>>
    {
        private readonly IUnitOfWork _unitOfWork;

        private readonly ILogger<AlteraProdutoHandler> _logger;

        public AlteraProdutoHandler(IUnitOfWork unitOfWork, ILogger<AlteraProdutoHandler> logger)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async Task<Response<AlteraProdutoResult>> Handle(AlteraProdutoCommand command)
        {
            var response = new Response<AlteraProdutoResult>();

            try
            {
                _unitOfWork.BeginTransaction();
                var validator = new AlteraProdutoValidator();
                var validateResult = validator.Validate(command);

                if (!validateResult.IsValid)
                {
                    response.AddErros(validateResult.Errors.ToCustomValidationFailure());
                    return response;
                }

                var linhasAfetadas = await _unitOfWork.Produto.Alterar(
                    new Entidades.Produto(command), command.EmpresaIdSolicitante);

                // Zero linhas: produto inexistente ou de outra empresa. Mesma mensagem nos dois
                // casos, pra nao confirmar ao atacante que o id existe.
                if (linhasAfetadas == 0)
                {
                    _unitOfWork.Rollback();
                    response.AddErro("Produto não encontrado.");
                    return response;
                }

                response.AddValue(new AlteraProdutoResult());
                _unitOfWork.Commit();
            }
            catch (System.Exception ex)
            {
                    _unitOfWork.Rollback();
                
                response.AddErro(TratamentoErro.Tratar(ex, _logger, nameof(AlteraProdutoHandler)));
            }

            return response;
        }
    }
}


