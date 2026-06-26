using ProjetoMetaMensagem.Dominio.Common;
using ProjetoMetaMensagem.Dominio.Interfaces;
using ProjetoMetaMensagem.Dominio.Interfaces.Mediator;
using System.Threading.Tasks;

namespace ProjetoMetaMensagem.Dominio.UseCases.Produto.DeletaProduto
{
    public class DeletaProdutoHandler : IRequestHandler<DeletaProdutoCommand, Response<DeletaProdutoResult>>
    {
        private readonly IUnitOfWork _unitOfWork;

        public DeletaProdutoHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<Response<DeletaProdutoResult>> Handle(DeletaProdutoCommand command)
        {
            var response = new Response<DeletaProdutoResult>();

            try
            {
                await _unitOfWork.Produto.Excluir(command.Id);

                response.AddValue(new DeletaProdutoResult());
            }
            catch (System.Exception ex)
            {
                response.AddErro($"Erro: {ex.Message}");
            }

            return response;
        }
    }
}
