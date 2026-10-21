using Microsoft.Extensions.Logging;
using ProjetoMetaMensagem.Dominio.Common;
using ProjetoMetaMensagem.Dominio.Help.Error;
using ProjetoMetaMensagem.Dominio.Interfaces;
using ProjetoMetaMensagem.Dominio.Interfaces.Mediator;
using System;
using System.Threading.Tasks;

namespace ProjetoMetaMensagem.Dominio.UseCases.Contato.ReativaContato
{
    // Volta um contato inativado pela exclusao (ver ContatoRepository.Excluir). O controller
    // enfileira o gatilho de cadastro de novo, como se o contato tivesse acabado de entrar.
    public class ReativaContatoHandler : IRequestHandler<ReativaContatoCommand, Response<ReativaContatoResult>>
    {
        private readonly IUnitOfWork _unitOfWork;

        private readonly ILogger<ReativaContatoHandler> _logger;

        public ReativaContatoHandler(IUnitOfWork unitOfWork, ILogger<ReativaContatoHandler> logger)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async Task<Response<ReativaContatoResult>> Handle(ReativaContatoCommand command)
        {
            var response = new Response<ReativaContatoResult>();

            try
            {
                var validator = new ReativaContatoValidator();
                var validateResult = validator.Validate(command);

                if (!validateResult.IsValid)
                {
                    response.AddErros(validateResult.Errors.ToCustomValidationFailure());
                    return response;
                }

                _unitOfWork.BeginTransaction();

                // Mesma mensagem pra inexistente, de outra empresa ou ja ativo (ver DeletaContatoHandler).
                var linhasAfetadas = await _unitOfWork.Contato.Reativar(command.Id, command.EmpresaIdSolicitante);
                if (linhasAfetadas == 0)
                {
                    _unitOfWork.Rollback();
                    response.AddErro("Contato não encontrado.");
                    return response;
                }

                var contato = await _unitOfWork.Contato.ObterPorId(command.Id, command.EmpresaIdSolicitante);

                response.AddValue(new ReativaContatoResult { ContatoId = command.Id, EmpresaId = contato!.EmpresaId });
                _unitOfWork.Commit();
            }
            catch (Exception ex)
            {
                _unitOfWork.Rollback();

                response.AddErroServico(ex, _logger, nameof(ReativaContatoHandler));
            }

            return response;
        }
    }
}
