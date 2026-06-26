using ProjetoMetaMensagem.Dominio.Common;
using ProjetoMetaMensagem.Dominio.Interfaces;
using ProjetoMetaMensagem.Dominio.Interfaces.Mediator;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjetoMetaMensagem.Dominio.UseCases.Flows.ListaFlows
{
    public class ListaFlowsHandler : IRequestHandler<ListaFlowsCommand, Response<List<ListaFlowsResult>>>
    {
        private readonly IUnitOfWork _unitOfWork;
        public ListaFlowsHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<Response<List<ListaFlowsResult>>> Handle(ListaFlowsCommand command)
        {
            var response = new Response<List<ListaFlowsResult>>();

            try
            {
                var listaResultados = new List<ListaFlowsResult>();

                //var validator = new CriaClienteValidator();
                //var validateResult = validator.Validate(request);

                //if (!validateResult.IsValid)
                //{
                //    response.AddErros(validateResult.Errors.ToCustomValidationFailure());
                //    return response;
                //}

                var flows = await _unitOfWork.Flow.ObterTodosPorEmpresa(command.IdEmpresa);

                foreach (var flow in flows)
                {
                    listaResultados.Add(new ListaFlowsResult(flow));
                }

                response.AddValue(listaResultados);
            }
            catch (Exception ex)
            {
                response.AddErro($"Erro: {ex.Message}");
            }

            return response;
        }
    }
}
