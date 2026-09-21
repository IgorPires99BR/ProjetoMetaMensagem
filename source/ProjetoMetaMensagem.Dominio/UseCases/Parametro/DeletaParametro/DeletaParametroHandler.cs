using Microsoft.Extensions.Logging;
using ProjetoMetaMensagem.Dominio.Common;
using ProjetoMetaMensagem.Dominio.Help.Error;
using ProjetoMetaMensagem.Dominio.Interfaces;
using ProjetoMetaMensagem.Dominio.Interfaces.Mediator;
using System;
using System.Threading.Tasks;

namespace ProjetoMetaMensagem.Dominio.UseCases.Parametro.DeletaParametro
{
    public class DeletaParametroHandler : IRequestHandler<DeletaParametroCommand, Response<DeletaParametroResult>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<DeletaParametroHandler> _logger;

        public DeletaParametroHandler(IUnitOfWork unitOfWork, ILogger<DeletaParametroHandler> logger)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async Task<Response<DeletaParametroResult>> Handle(DeletaParametroCommand command)
        {
            var response = new Response<DeletaParametroResult>();

            try
            {
                var validateResult = new DeletaParametroValidator().Validate(command);
                if (!validateResult.IsValid)
                {
                    response.AddErros(validateResult.Errors.ToCustomValidationFailure());
                    return response;
                }

                // Um agendamento que aponta pra este parametro passaria a disparar com a
                // variavel vazia (a Meta recusa) sem ninguem perceber ate a proxima execucao.
                var emUso = await _unitOfWork.Parametro.ContarAgendamentosUsando(command.Id);
                if (emUso > 0)
                {
                    response.AddErro($"Este parâmetro está em uso por {emUso} agendamento(s). Troque a variável nesses agendamentos antes de excluir.");
                    return response;
                }

                _unitOfWork.BeginTransaction();

                var linhasAfetadas = await _unitOfWork.Parametro.Excluir(command.Id, command.EmpresaIdSolicitante);

                if (linhasAfetadas == 0)
                {
                    _unitOfWork.Rollback();
                    response.AddErro("Parâmetro não encontrado.");
                    return response;
                }

                response.AddValue(new DeletaParametroResult());
                _unitOfWork.Commit();
            }
            catch (Exception ex)
            {
                _unitOfWork.Rollback();
                response.AddErroServico(ex, _logger, nameof(DeletaParametroHandler));
            }

            return response;
        }
    }
}
