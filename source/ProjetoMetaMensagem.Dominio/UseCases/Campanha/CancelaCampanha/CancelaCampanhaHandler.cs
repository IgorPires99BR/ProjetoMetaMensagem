using ProjetoMetaMensagem.Dominio.Common;
using ProjetoMetaMensagem.Dominio.Interfaces;
using ProjetoMetaMensagem.Dominio.Interfaces.Mediator;

namespace ProjetoMetaMensagem.Dominio.UseCases.Campanha.CancelaCampanha
{
    public class CancelaCampanhaHandler : IRequestHandler<CancelaCampanhaCommand, Response<CancelaCampanhaResult>>
    {
        private readonly IUnitOfWork _unitOfWork;

        public CancelaCampanhaHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<Response<CancelaCampanhaResult>> Handle(CancelaCampanhaCommand command)
        {
            var response = new Response<CancelaCampanhaResult>();

            try
            {
                _unitOfWork.BeginTransaction();
                await _unitOfWork.Campanha.AtualizarStatus(command.Id, "CANCELADA");

                response.AddValue(new CancelaCampanhaResult());
                _unitOfWork.Commit();
            }
            catch (Exception ex)
            {
                    _unitOfWork.Rollback();
                
                response.AddErro($"Erro ao cancelar campanha: {ex.Message}");
            }

            return response;
        }
    }
}


