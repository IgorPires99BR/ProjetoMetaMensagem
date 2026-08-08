using ProjetoMetaMensagem.Dominio.Help.Error;
using Microsoft.Extensions.Logging;
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
        private readonly ILogger<ListaConversaPorIdHandler> _logger;

        public ListaConversaPorIdHandler(IUnitOfWork unitOfWork, ILogger<ListaConversaPorIdHandler> logger)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async Task<Response<List<ListaConversaPorIdResult>>> Handle(ListaConversaPorIdCommand command)
        {
            var response = new Response<List<ListaConversaPorIdResult>>();

            try
            {
                var listaResultados = new List<ListaConversaPorIdResult>();

                //var validator = new CriaClienteValidator();
                //var validateResult = validator.Validate(request);

                //if (!validateResult.IsValid)
                //{
                //    response.AddErros(validateResult.Errors.ToCustomValidationFailure());
                //    return response;
                //}

                var conversations = await _unitOfWork.ConversationsRepository.Obter(command.CompanyId);

                foreach (var conversation in conversations)
                {
                    listaResultados.Add(new ListaConversaPorIdResult(conversation));
                }

                response.AddValue(listaResultados);
            }
            catch (Exception ex)
            {
                response.AddErroServico(ex, _logger, nameof(ListaConversaPorIdHandler));
            }

            return response;
        }
    }
}
