using ProjetoMetaMensagem.Dominio.Help.Error;
using Microsoft.Extensions.Logging;
using ProjetoMetaMensagem.Dominio.Common;
using ProjetoMetaMensagem.Dominio.Interfaces;
using ProjetoMetaMensagem.Dominio.Interfaces.Mediator;
using System.Threading.Tasks;

namespace ProjetoMetaMensagem.Dominio.UseCases.Produto.DeletaProduto
{
    public class DeletaProdutoHandler : IRequestHandler<DeletaProdutoCommand, Response<DeletaProdutoResult>>
    {
        private readonly IUnitOfWork _unitOfWork;

        private readonly ILogger<DeletaProdutoHandler> _logger;

        public DeletaProdutoHandler(IUnitOfWork unitOfWork, ILogger<DeletaProdutoHandler> logger)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async Task<Response<DeletaProdutoResult>> Handle(DeletaProdutoCommand command)
        {
            var response = new Response<DeletaProdutoResult>();

            try
            {
                _unitOfWork.BeginTransaction();
                var linhasAfetadas = await _unitOfWork.Produto.Excluir(command.Id, command.EmpresaIdSolicitante);

                // Zero linhas significa que o produto nao existe OU pertence a outra empresa.
                // As duas situacoes devolvem a mesma mensagem de proposito: dizer "existe, mas
                // nao e seu" ja entregaria ao atacante que aquele id e valido.
                if (linhasAfetadas == 0)
                {
                    _unitOfWork.Rollback();
                    response.AddErro("Produto não encontrado.");
                    return response;
                }

                response.AddValue(new DeletaProdutoResult());
                _unitOfWork.Commit();
            }
            catch (System.Exception ex)
            {
                    _unitOfWork.Rollback();
                
                response.AddErro(TratamentoErro.Tratar(ex, _logger, nameof(DeletaProdutoHandler)));
            }

            return response;
        }
    }
}


