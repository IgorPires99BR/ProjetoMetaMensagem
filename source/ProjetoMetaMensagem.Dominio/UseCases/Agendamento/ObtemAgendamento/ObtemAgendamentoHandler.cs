using ProjetoMetaMensagem.Dominio.Help.Error;
using Microsoft.Extensions.Logging;
using ProjetoMetaMensagem.Dominio.Common;
using ProjetoMetaMensagem.Dominio.Interfaces;
using ProjetoMetaMensagem.Dominio.Interfaces.Mediator;
using System;
using System.Linq;

namespace ProjetoMetaMensagem.Dominio.UseCases.Agendamento.ObtemAgendamento
{
    public class ObtemAgendamentoHandler : IRequestHandler<ObtemAgendamentoCommand, Response<ObtemAgendamentoResult>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<ObtemAgendamentoHandler> _logger;

        public ObtemAgendamentoHandler(IUnitOfWork unitOfWork, ILogger<ObtemAgendamentoHandler> logger)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async Task<Response<ObtemAgendamentoResult>> Handle(ObtemAgendamentoCommand command)
        {
            var response = new Response<ObtemAgendamentoResult>();

            var validator = new ObtemAgendamentoValidator();
            var validateResult = validator.Validate(command);
            if (!validateResult.IsValid)
            {
                response.AddErros(validateResult.Errors.ToCustomValidationFailure());
                return response;
            }

            try
            {
                var agendamento = await _unitOfWork.Agendamento.ObterPorId(command.Id, command.EmpresaIdSolicitante);
                if (agendamento == null)
                {
                    response.AddErro("Agendamento não encontrado.");
                    return response;
                }

                var contatoIds = (await _unitOfWork.Agendamento.ObterContatoIds(agendamento.Id)).ToList();

                response.AddValue(new ObtemAgendamentoResult
                {
                    Id = agendamento.Id,
                    Nome = agendamento.Nome,
                    TemplateId = agendamento.TemplateId,
                    TipoRecorrencia = agendamento.TipoRecorrencia,
                    DataInicio = agendamento.DataInicio,
                    DataFim = agendamento.DataFim,
                    ProximaExecucao = agendamento.ProximaExecucao,
                    Ativo = agendamento.Ativo,
                    ContatoIds = contatoIds
                });
            }
            catch (Exception ex)
            {
                response.AddErroServico(ex, _logger, nameof(ObtemAgendamentoHandler));
            }

            return response;
        }
    }
}
