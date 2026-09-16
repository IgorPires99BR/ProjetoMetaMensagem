using ProjetoMetaMensagem.Dominio.Help.Error;
using Microsoft.Extensions.Logging;
using ProjetoMetaMensagem.Dominio.Common;
using ProjetoMetaMensagem.Dominio.Interfaces;
using ProjetoMetaMensagem.Dominio.Interfaces.Mediator;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ProjetoMetaMensagem.Dominio.UseCases.Agendamento.ListaExecucoesAgendamento
{
    public class ListaExecucoesAgendamentoHandler : IRequestHandler<ListaExecucoesAgendamentoCommand, Response<List<ListaExecucoesAgendamentoResult>>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<ListaExecucoesAgendamentoHandler> _logger;

        public ListaExecucoesAgendamentoHandler(IUnitOfWork unitOfWork, ILogger<ListaExecucoesAgendamentoHandler> logger)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async Task<Response<List<ListaExecucoesAgendamentoResult>>> Handle(ListaExecucoesAgendamentoCommand command)
        {
            var response = new Response<List<ListaExecucoesAgendamentoResult>>();

            var validator = new ListaExecucoesAgendamentoValidator();
            var validateResult = validator.Validate(command);
            if (!validateResult.IsValid)
            {
                response.AddErros(validateResult.Errors.ToCustomValidationFailure());
                return response;
            }

            try
            {
                var execucoes = await _unitOfWork.Agendamento.ListarExecucoes(command.AgendamentoId, command.EmpresaIdSolicitante);

                var listaResult = execucoes.Select(e => new ListaExecucoesAgendamentoResult
                {
                    Id = e.Id,
                    DataExecucao = e.DataExecucao,
                    TotalContatos = e.TotalContatos,
                    Sucessos = e.Sucessos,
                    Falhas = e.Falhas,
                    Status = e.Status
                }).ToList();

                response.AddValue(listaResult);
            }
            catch (Exception ex)
            {
                response.AddErroServico(ex, _logger, nameof(ListaExecucoesAgendamentoHandler));
            }

            return response;
        }
    }
}
