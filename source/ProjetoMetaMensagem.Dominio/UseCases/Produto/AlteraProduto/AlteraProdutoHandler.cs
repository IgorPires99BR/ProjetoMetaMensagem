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

        public AlteraProdutoHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
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

                await _unitOfWork.Produto.Alterar(new Entidades.Produto(command));

                response.AddValue(new AlteraProdutoResult());
                _unitOfWork.Commit();
            }
            catch (System.Exception ex)
            {
                    _unitOfWork.Rollback();
                
                response.AddErro($"Erro: {ex.Message}");
            }

            return response;
        }
    }
}


