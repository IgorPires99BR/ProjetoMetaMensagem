using ProjetoMetaMensagem.Dominio.Help.Error;
using Microsoft.Extensions.Logging;
using ProjetoMetaMensagem.Dominio.Common;
using ProjetoMetaMensagem.Dominio.Interfaces;
using ProjetoMetaMensagem.Dominio.Interfaces.Mediator;
using System;

namespace ProjetoMetaMensagem.Dominio.UseCases.Agendamento.DeletaAgendamento
{
    public class DeletaAgendamentoHandler : IRequestHandler<DeletaAgendamentoCommand, Response<DeletaAgendamentoResult>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<DeletaAgendamentoHandler> _logger;

        public DeletaAgendamentoHandler(IUnitOfWork unitOfWork, ILogger<DeletaAgendamentoHandler> logger)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async Task<Response<DeletaAgendamentoResult>> Handle(DeletaAgendamentoCommand command)
        {
            var response = new Response<DeletaAgendamentoResult>();

            var validator = new DeletaAgendamentoValidator();
            var validateResult = validator.Validate(command);
            if (!validateResult.IsValid)
            {
                response.AddErros(validateResult.Errors.ToCustomValidationFailure());
                return response;
            }

            try
            {
                _unitOfWork.BeginTransaction();
                var linhasAfetadas = await _unitOfWork.Agendamento.Deletar(command.Id, command.EmpresaIdSolicitante);

                if (linhasAfetadas == 0)
                {
                    _unitOfWork.Rollback();
                    response.AddErro("Agendamento não encontrado.");
                    return response;
                }

                response.AddValue(new DeletaAgendamentoResult());
                _unitOfWork.Commit();
            }
            catch (Exception ex)
            {
                _unitOfWork.Rollback();
                response.AddErroServico(ex, _logger, nameof(DeletaAgendamentoHandler));
            }

            return response;
        }
    }
}
