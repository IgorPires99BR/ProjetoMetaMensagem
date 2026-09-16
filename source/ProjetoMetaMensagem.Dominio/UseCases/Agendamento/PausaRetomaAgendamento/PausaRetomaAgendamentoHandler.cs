using ProjetoMetaMensagem.Dominio.Help.Error;
using Microsoft.Extensions.Logging;
using ProjetoMetaMensagem.Dominio.Common;
using ProjetoMetaMensagem.Dominio.Interfaces;
using ProjetoMetaMensagem.Dominio.Interfaces.Mediator;
using System;

namespace ProjetoMetaMensagem.Dominio.UseCases.Agendamento.PausaRetomaAgendamento
{
    public class PausaRetomaAgendamentoHandler : IRequestHandler<PausaRetomaAgendamentoCommand, Response<PausaRetomaAgendamentoResult>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<PausaRetomaAgendamentoHandler> _logger;

        public PausaRetomaAgendamentoHandler(IUnitOfWork unitOfWork, ILogger<PausaRetomaAgendamentoHandler> logger)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async Task<Response<PausaRetomaAgendamentoResult>> Handle(PausaRetomaAgendamentoCommand command)
        {
            var response = new Response<PausaRetomaAgendamentoResult>();

            var validator = new PausaRetomaAgendamentoValidator();
            var validateResult = validator.Validate(command);
            if (!validateResult.IsValid)
            {
                response.AddErros(validateResult.Errors.ToCustomValidationFailure());
                return response;
            }

            try
            {
                _unitOfWork.BeginTransaction();
                var linhasAfetadas = await _unitOfWork.Agendamento.AtualizarStatus(
                    command.Id, command.Ativo, command.EmpresaIdSolicitante);

                if (linhasAfetadas == 0)
                {
                    _unitOfWork.Rollback();
                    response.AddErro("Agendamento não encontrado.");
                    return response;
                }

                response.AddValue(new PausaRetomaAgendamentoResult());
                _unitOfWork.Commit();
            }
            catch (Exception ex)
            {
                _unitOfWork.Rollback();
                response.AddErroServico(ex, _logger, nameof(PausaRetomaAgendamentoHandler));
            }

            return response;
        }
    }
}
