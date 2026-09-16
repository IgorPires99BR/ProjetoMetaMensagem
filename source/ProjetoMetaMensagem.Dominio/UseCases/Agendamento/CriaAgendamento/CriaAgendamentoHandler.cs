using ProjetoMetaMensagem.Dominio.Help.Error;
using Microsoft.Extensions.Logging;
using ProjetoMetaMensagem.Dominio.Common;
using ProjetoMetaMensagem.Dominio.Interfaces;
using ProjetoMetaMensagem.Dominio.Interfaces.Mediator;
using ProjetoMetaMensagem.Dominio.Servicos;
using System;
using System.Linq;

namespace ProjetoMetaMensagem.Dominio.UseCases.Agendamento.CriaAgendamento
{
    public class CriaAgendamentoHandler : IRequestHandler<CriaAgendamentoCommand, Response<CriaAgendamentoResult>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<CriaAgendamentoHandler> _logger;

        public CriaAgendamentoHandler(IUnitOfWork unitOfWork, ILogger<CriaAgendamentoHandler> logger)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async Task<Response<CriaAgendamentoResult>> Handle(CriaAgendamentoCommand command)
        {
            var response = new Response<CriaAgendamentoResult>();

            var validator = new CriaAgendamentoValidator();
            var validateResult = validator.Validate(command);
            if (!validateResult.IsValid)
            {
                response.AddErros(validateResult.Errors.ToCustomValidationFailure());
                return response;
            }

            try
            {
                // ContatoIds vem do corpo da requisicao: sem conferir que cada um pertence a
                // EmpresaId (mesmo cuidado de CriaCampanhaHandler), um usuario autenticado de
                // uma empresa conseguiria agendar disparo real pra contatos de outra empresa.
                var contatosValidos = await _unitOfWork.Contato.ObterPorIds(command.EmpresaId, command.ContatoIds ?? new System.Collections.Generic.List<Guid>());
                if (contatosValidos.Count() != (command.ContatoIds?.Count ?? 0))
                {
                    response.AddErro("Um ou mais contatos informados não pertencem a esta empresa.");
                    return response;
                }

                _unitOfWork.BeginTransaction();

                var agendamento = new Entidades.Agendamento
                {
                    EmpresaId = command.EmpresaId,
                    Nome = command.Nome,
                    TemplateId = command.TemplateId,
                    TipoRecorrencia = command.TipoRecorrencia,
                    DataInicio = command.DataInicio,
                    DataFim = command.DataFim,
                    DataReferencia = command.DataReferencia,
                    ProximaExecucao = AgendamentoRecorrencia.CalcularPrimeiraExecucao(
                        command.DataReferencia, command.TipoRecorrencia, command.DiasSemana),
                    UsuarioCriacaoId = command.UsuarioCriacaoId,
                    Variaveis = command.Variaveis ?? new System.Collections.Generic.List<Entidades.AgendamentoVariavelDto>(),
                    DiasSemanaLista = command.DiasSemana ?? new System.Collections.Generic.List<int>(),
                    DiaDoMes = command.DiaDoMes
                };

                var agendamentoId = await _unitOfWork.Agendamento.Incluir(agendamento);

                var vinculos = command.ContatoIds
                    .Select(contatoId => new Entidades.AgendamentoContato
                    {
                        Id = Guid.NewGuid(),
                        AgendamentoId = agendamentoId,
                        ContatoId = contatoId
                    })
                    .ToList();

                await _unitOfWork.Agendamento.IncluirContatos(vinculos);

                response.AddValue(new CriaAgendamentoResult { Id = agendamentoId });
                _unitOfWork.Commit();
            }
            catch (Exception ex)
            {
                _unitOfWork.Rollback();
                response.AddErroServico(ex, _logger, nameof(CriaAgendamentoHandler));
            }

            return response;
        }
    }
}
