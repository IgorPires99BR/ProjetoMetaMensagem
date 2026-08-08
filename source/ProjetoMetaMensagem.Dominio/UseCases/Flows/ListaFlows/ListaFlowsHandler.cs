using ProjetoMetaMensagem.Dominio.Help.Error;
using Microsoft.Extensions.Logging;
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
        private readonly ILogger<ListaFlowsHandler> _logger;

        public ListaFlowsHandler(IUnitOfWork unitOfWork, ILogger<ListaFlowsHandler> logger)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
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
                response.AddErro(TratamentoErro.Tratar(ex, _logger, nameof(ListaFlowsHandler)));
            }

            return response;
        }
    }
}
