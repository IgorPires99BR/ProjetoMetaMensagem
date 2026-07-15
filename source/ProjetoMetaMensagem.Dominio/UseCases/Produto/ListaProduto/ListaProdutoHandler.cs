using ProjetoMetaMensagem.Dominio.Common;
using ProjetoMetaMensagem.Dominio.Interfaces;
using ProjetoMetaMensagem.Dominio.Interfaces.Mediator;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ProjetoMetaMensagem.Dominio.UseCases.Produto.ListaProduto
{
    public class ListaProdutoHandler : IRequestHandler<ListaProdutoCommand, Response<List<ListaProdutoResult>>>
    {
        private readonly IUnitOfWork _unitOfWork;

        public ListaProdutoHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<Response<List<ListaProdutoResult>>> Handle(ListaProdutoCommand command)
        {
            var response = new Response<List<ListaProdutoResult>>();

            try
            {
                var listaResultados = new List<ListaProdutoResult>();

                var produtos = await _unitOfWork.Produto.ListarPorEmpresa(command.EmpresaId);

                foreach (var produto in produtos)
                {
                    listaResultados.Add(new ListaProdutoResult(produto));
                }

                response.AddValue(listaResultados);
            }
            catch (System.Exception ex)
            {
                response.AddErro($"Erro: {ex.Message}");
            }

            return response;
        }
    }
}
