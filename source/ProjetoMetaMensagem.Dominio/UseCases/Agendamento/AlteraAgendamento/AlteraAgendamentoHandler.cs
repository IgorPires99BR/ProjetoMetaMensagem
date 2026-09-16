using ProjetoMetaMensagem.Dominio.Help.Error;
using Microsoft.Extensions.Logging;
using ProjetoMetaMensagem.Dominio.Common;
using ProjetoMetaMensagem.Dominio.Interfaces;
using ProjetoMetaMensagem.Dominio.Interfaces.Mediator;
using System;
using System.Linq;

namespace ProjetoMetaMensagem.Dominio.UseCases.Agendamento.AlteraAgendamento
{
    public class AlteraAgendamentoHandler : IRequestHandler<AlteraAgendamentoCommand, Response<AlteraAgendamentoResult>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<AlteraAgendamentoHandler> _logger;

        public AlteraAgendamentoHandler(IUnitOfWork unitOfWork, ILogger<AlteraAgendamentoHandler> logger)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async Task<Response<AlteraAgendamentoResult>> Handle(AlteraAgendamentoCommand command)
        {
            var response = new Response<AlteraAgendamentoResult>();

            var validator = new AlteraAgendamentoValidator();
            var validateResult = validator.Validate(command);
            if (!validateResult.IsValid)
            {
                response.AddErros(validateResult.Errors.ToCustomValidationFailure());
                return response;
            }

            try
            {
                var existente = await _unitOfWork.Agendamento.ObterPorId(command.Id, command.EmpresaIdSolicitante);
                if (existente == null)
                {
                    // Nao existe OU pertence a outra empresa -- mesma mensagem pros dois casos
                    // de proposito (nao entrega ao atacante que o id e valido).
                    response.AddErro("Agendamento não encontrado.");
                    return response;
                }

                var contatosValidos = await _unitOfWork.Contato.ObterPorIds(existente.EmpresaId, command.ContatoIds ?? new System.Collections.Generic.List<Guid>());
                if (contatosValidos.Count() != (command.ContatoIds?.Count ?? 0))
                {
                    response.AddErro("Um ou mais contatos informados não pertencem a esta empresa.");
                    return response;
                }

                // So reinicia o ciclo de recorrencia se a data/hora de referencia ou o tipo
                // mudaram -- editar so o nome/contatos nao deve adiantar nem atrasar o proximo disparo.
                var proximaExecucao = existente.ProximaExecucao;
                if (existente.DataReferencia != command.DataReferencia || existente.TipoRecorrencia != command.TipoRecorrencia)
                {
                    proximaExecucao = command.DataReferencia;
                }

                _unitOfWork.BeginTransaction();

                existente.Nome = command.Nome;
                existente.TemplateId = command.TemplateId;
                existente.TipoRecorrencia = command.TipoRecorrencia;
                existente.DataInicio = command.DataInicio;
                existente.DataFim = command.DataFim;
                existente.DataReferencia = command.DataReferencia;
                existente.ProximaExecucao = proximaExecucao;
                existente.DataAtualizacao = DateTime.Now;
                existente.Variaveis = command.Variaveis ?? new System.Collections.Generic.List<Entidades.AgendamentoVariavelDto>();

                var linhasAfetadas = await _unitOfWork.Agendamento.Atualizar(existente, command.EmpresaIdSolicitante);
                if (linhasAfetadas == 0)
                {
                    _unitOfWork.Rollback();
                    response.AddErro("Agendamento não encontrado.");
                    return response;
                }

                await _unitOfWork.Agendamento.SubstituirContatos(command.Id, command.ContatoIds);

                response.AddValue(new AlteraAgendamentoResult());
                _unitOfWork.Commit();
            }
            catch (Exception ex)
            {
                _unitOfWork.Rollback();
                response.AddErroServico(ex, _logger, nameof(AlteraAgendamentoHandler));
            }

            return response;
        }
    }
}
