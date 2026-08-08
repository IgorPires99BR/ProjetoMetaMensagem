using ProjetoMetaMensagem.Dominio.Help.Error;
using Microsoft.Extensions.Logging;
using ProjetoMetaMensagem.Dominio.Common;
using ProjetoMetaMensagem.Dominio.Interfaces;
using ProjetoMetaMensagem.Dominio.Interfaces.Mediator;

namespace ProjetoMetaMensagem.Dominio.UseCases.Campanha.CancelaCampanha
{
    public class CancelaCampanhaHandler : IRequestHandler<CancelaCampanhaCommand, Response<CancelaCampanhaResult>>
    {
        private readonly IUnitOfWork _unitOfWork;

        private readonly ILogger<CancelaCampanhaHandler> _logger;

        public CancelaCampanhaHandler(IUnitOfWork unitOfWork, ILogger<CancelaCampanhaHandler> logger)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async Task<Response<CancelaCampanhaResult>> Handle(CancelaCampanhaCommand command)
        {
            var response = new Response<CancelaCampanhaResult>();

            try
            {
                _unitOfWork.BeginTransaction();
                var linhasAfetadas = await _unitOfWork.Campanha.AtualizarStatus(
                    command.Id, "CANCELADA", command.EmpresaIdSolicitante);

                // Zero linhas significa que a campanha nao existe OU pertence a outra empresa.
                // As duas situacoes devolvem a mesma mensagem de proposito: dizer "existe, mas
                // nao e sua" ja entregaria ao atacante que aquele id e valido.
                if (linhasAfetadas == 0)
                {
                    _unitOfWork.Rollback();
                    response.AddErro("Campanha não encontrada.");
                    return response;
                }

                response.AddValue(new CancelaCampanhaResult());
                _unitOfWork.Commit();
            }
            catch (Exception ex)
            {
                    _unitOfWork.Rollback();
                
                response.AddErroServico(ex, _logger, nameof(CancelaCampanhaHandler));
            }

            return response;
        }
    }
}


