using ProjetoMetaMensagem.Dominio.Help.Error;
using Microsoft.Extensions.Logging;
using ProjetoMetaMensagem.Dominio.Common;
using ProjetoMetaMensagem.Dominio.Interfaces;
using ProjetoMetaMensagem.Dominio.Interfaces.Mediator;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ProjetoMetaMensagem.Dominio.UseCases.Agendamento.ListaAgendamento
{
    public class ListaAgendamentoHandler : IRequestHandler<ListaAgendamentoCommand, Response<List<ListaAgendamentoResult>>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<ListaAgendamentoHandler> _logger;

        public ListaAgendamentoHandler(IUnitOfWork unitOfWork, ILogger<ListaAgendamentoHandler> logger)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async Task<Response<List<ListaAgendamentoResult>>> Handle(ListaAgendamentoCommand command)
        {
            var response = new Response<List<ListaAgendamentoResult>>();

            var validator = new ListaAgendamentoValidator();
            var validateResult = validator.Validate(command);
            if (!validateResult.IsValid)
            {
                response.AddErros(validateResult.Errors.ToCustomValidationFailure());
                return response;
            }

            try
            {
                var agendamentos = await _unitOfWork.Agendamento.Listar(command.EmpresaId);

                var listaResult = agendamentos.Select(a => new ListaAgendamentoResult
                {
                    Id = a.Id,
                    Nome = a.Nome,
                    TemplateId = a.TemplateId,
                    NomeTemplate = a.NomeTemplate,
                    TipoRecorrencia = a.TipoRecorrencia,
                    DataInicio = a.DataInicio,
                    DataFim = a.DataFim,
                    ProximaExecucao = a.ProximaExecucao,
                    Ativo = a.Ativo,
                    TotalContatos = a.TotalContatos
                }).ToList();

                response.AddValue(listaResult);
            }
            catch (Exception ex)
            {
                response.AddErroServico(ex, _logger, nameof(ListaAgendamentoHandler));
            }

            return response;
        }
    }
}
