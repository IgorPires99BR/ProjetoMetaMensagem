using Microsoft.Extensions.Logging;
using ProjetoMetaMensagem.Dominio.Common;
using ProjetoMetaMensagem.Dominio.Help.Error;
using ProjetoMetaMensagem.Dominio.Interfaces;
using ProjetoMetaMensagem.Dominio.Interfaces.Mediator;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ProjetoMetaMensagem.Dominio.UseCases.Template.ListaGatilhosTemplate
{
    public class ListaGatilhosTemplateHandler : IRequestHandler<ListaGatilhosTemplateCommand, Response<List<ListaGatilhosTemplateResult>>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<ListaGatilhosTemplateHandler> _logger;

        public ListaGatilhosTemplateHandler(IUnitOfWork unitOfWork, ILogger<ListaGatilhosTemplateHandler> logger)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async Task<Response<List<ListaGatilhosTemplateResult>>> Handle(ListaGatilhosTemplateCommand command)
        {
            var response = new Response<List<ListaGatilhosTemplateResult>>();

            if (command.EmpresaIdSolicitante.HasValue)
                command.EmpresaId = command.EmpresaIdSolicitante.Value;

            var validateResult = new ListaGatilhosTemplateValidator().Validate(command);
            if (!validateResult.IsValid)
            {
                response.AddErros(validateResult.Errors.ToCustomValidationFailure());
                return response;
            }

            try
            {
                var gatilhos = await _unitOfWork.Template.ObterGatilhosPorEmpresa(command.EmpresaId);
                response.AddValue(gatilhos.Select(g => new ListaGatilhosTemplateResult(g)).ToList());
            }
            catch (Exception ex)
            {
                response.AddErroServico(ex, _logger, nameof(ListaGatilhosTemplateHandler));
            }

            return response;
        }
    }
}
