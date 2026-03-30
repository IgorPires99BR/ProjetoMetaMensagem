using ProjetoMetaMensagem.Dominio.Common;
using ProjetoMetaMensagem.Dominio.Interfaces;
using ProjetoMetaMensagem.Dominio.Interfaces.Mediator;
using ProjetoMetaMensagem.Dominio.UseCases.Flows.ListaFlows;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjetoMetaMensagem.Dominio.UseCases.Leads.ListaConversas
{
    public class ListaConversaPorIdHandler : IRequestHandler<ListaConversaPorIdCommand, Response<List<ListaConversaPorIdResult>>>
    {
        private readonly IUnitOfWork _unitOfWork;
        public ListaConversaPorIdHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<Response<List<ListaConversaPorIdResult>>> Handle(ListaConversaPorIdCommand command)
        {
            var response = new Response<List<ListaConversaPorIdResult>>();

            var listaResultados = new List<ListaConversaPorIdResult>();

            //var validator = new CriaClienteValidator();
            //var validateResult = validator.Validate(request);

            //if (!validateResult.IsValid)
            //{
            //    response.AddErros(validateResult.Errors.ToCustomValidationFailure());
            //    return response;
            //}

            var flows = await _unitOfWork.FlowsRepository.Obtem(command.CompanyId);

            return response;
        }
    }
}
