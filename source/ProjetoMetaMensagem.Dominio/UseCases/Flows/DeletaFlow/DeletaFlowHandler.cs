using ProjetoMetaMensagem.Dominio.Common;
using ProjetoMetaMensagem.Dominio.Interfaces;
using ProjetoMetaMensagem.Dominio.Interfaces.Mediator;
using System;
using System.Threading.Tasks;

namespace ProjetoMetaMensagem.Dominio.UseCases.Flows.DeletaFlow
{
    public class DeletaFlowHandler : IRequestHandler<DeletaFlowCommand, Response<DeletaFlowResult>>
    {
        private readonly IUnitOfWork _unitOfWork;

        public DeletaFlowHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<Response<DeletaFlowResult>> Handle(DeletaFlowCommand command)
        {
            var response = new Response<DeletaFlowResult>();

            try
            {
                _unitOfWork.BeginTransaction();

                // Remove as etapas primeiro pra nao violar a FK (FlowEtapa.FlowId -> Flow.Id)
                await _unitOfWork.Flow.ExcluirEtapasPorFlowId(command.Id);
                await _unitOfWork.Flow.Excluir(command.Id);

                response.AddValue(new DeletaFlowResult());
                _unitOfWork.Commit();
            }
            catch (Exception ex)
            {
                _unitOfWork.Rollback();
                response.AddErro($"Erro ao excluir fluxo: {ex.Message}");
            }

            return response;
        }
    }
}
