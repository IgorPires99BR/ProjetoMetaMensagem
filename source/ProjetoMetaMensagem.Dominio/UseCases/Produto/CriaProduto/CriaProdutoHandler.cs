using ProjetoMetaMensagem.Dominio.Common;
using ProjetoMetaMensagem.Dominio.Interfaces;
using ProjetoMetaMensagem.Dominio.Interfaces.Mediator;
using ProjetoMetaMensagem.Dominio.Help.Error;
using System.Threading.Tasks;

namespace ProjetoMetaMensagem.Dominio.UseCases.Produto.CriaProduto
{
    public class CriaProdutoHandler : IRequestHandler<CriaProdutoCommand, Response<CriaProdutoResult>>
    {
        private readonly IUnitOfWork _unitOfWork;

        public CriaProdutoHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<Response<CriaProdutoResult>> Handle(CriaProdutoCommand command)
        {
            var response = new Response<CriaProdutoResult>();

            try
            {
                var validator = new CriaProdutoValidator();
                var validateResult = validator.Validate(command);

                if (!validateResult.IsValid)
                {
                    response.AddErros(validateResult.Errors.ToCustomValidationFailure());
                    return response;
                }

                var entity = new Entidades.Produto(command);
                var id = await _unitOfWork.Produto.Incluir(entity);

                response.AddValue(new CriaProdutoResult { Id = id });
            }
            catch (System.Exception ex)
            {
                response.AddErro($"Erro: {ex.Message}");
            }

            return response;
        }
    }
}
