using ProjetoMetaMensagem.Dominio.Help.Error;
using Microsoft.Extensions.Logging;
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

        private readonly ILogger<ListaProdutoHandler> _logger;

        public ListaProdutoHandler(IUnitOfWork unitOfWork, ILogger<ListaProdutoHandler> logger)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async Task<Response<List<ListaProdutoResult>>> Handle(ListaProdutoCommand command)
        {
            var response = new Response<List<ListaProdutoResult>>();

            try
            {
                var validator = new ListaProdutoValidator();
                var validateResult = validator.Validate(command);

                if (!validateResult.IsValid)
                {
                    response.AddErros(validateResult.Errors.ToCustomValidationFailure());
                    return response;
                }

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
                response.AddErroServico(ex, _logger, nameof(ListaProdutoHandler));
            }

            return response;
        }
    }
}
