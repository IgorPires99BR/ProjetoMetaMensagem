using ProjetoMetaMensagem.Dominio.Help.Error;
using Microsoft.Extensions.Logging;
using ProjetoMetaMensagem.Dominio.Common;
using ProjetoMetaMensagem.Dominio.Interfaces;
using ProjetoMetaMensagem.Dominio.Interfaces.Mediator;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace ProjetoMetaMensagem.Dominio.UseCases.Flows.DeletaFlow
{
    public class DeletaFlowHandler : IRequestHandler<DeletaFlowCommand, Response<DeletaFlowResult>>
    {
        private readonly IUnitOfWork _unitOfWork;

        private readonly ILogger<DeletaFlowHandler> _logger;

        public DeletaFlowHandler(IUnitOfWork unitOfWork, ILogger<DeletaFlowHandler> logger)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async Task<Response<DeletaFlowResult>> Handle(DeletaFlowCommand command)
        {
            var response = new Response<DeletaFlowResult>();

            try
            {
                _unitOfWork.BeginTransaction();

                // Excluir com conversa em andamento quebra a FK_ConvState_Etapa quando as
                // etapas forem removidas logo abaixo.
                var conversasDoFlow = await _unitOfWork.ConversationState.ObterPorFlow(command.Id);
                if (conversasDoFlow.Any(c => !c.Finalizado))
                {
                    _unitOfWork.Rollback();
                    response.AddErro("Este fluxo tem conversas em andamento e não pode ser excluído. Aguarde finalizarem antes de excluir.");
                    return response;
                }

                // Conversas ja finalizadas nao bloqueiam a exclusao (checado acima), mas a
                // LINHA continua no banco apontando (FK) pra uma FlowEtapa que esta prestes a
                // ser apagada. Sem isso, todo flow que ja rodou uma conversa - mesmo encerrada
                // - ficava travado pra sempre: o DELETE das etapas abaixo falhava com violacao
                // de FK assim que o primeiro cliente terminava o fluxo.
                foreach (var conversaFinalizada in conversasDoFlow)
                {
                    await _unitOfWork.ConversationState.Excluir(conversaFinalizada.Id);
                }

                // Remove as etapas primeiro pra nao violar a FK (FlowEtapa.FlowId -> Flow.Id).
                // O mesmo escopo vai nas duas chamadas, senao as etapas de um fluxo alheio
                // seriam apagadas antes do DELETE do Flow ser barrado.
                await _unitOfWork.Flow.ExcluirEtapasPorFlowId(command.Id, command.EmpresaIdSolicitante);
                var linhasAfetadas = await _unitOfWork.Flow.Excluir(command.Id, command.EmpresaIdSolicitante);

                // Zero linhas: fluxo inexistente ou de outra empresa. Mesma mensagem nos dois
                // casos, pra nao confirmar ao atacante que o id existe.
                if (linhasAfetadas == 0)
                {
                    _unitOfWork.Rollback();
                    response.AddErro("Fluxo não encontrado.");
                    return response;
                }

                response.AddValue(new DeletaFlowResult());
                _unitOfWork.Commit();
            }
            catch (Exception ex)
            {
                _unitOfWork.Rollback();
                response.AddErro(TratamentoErro.Tratar(ex, _logger, nameof(DeletaFlowHandler)));
            }

            return response;
        }
    }
}
