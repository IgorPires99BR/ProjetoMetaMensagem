using ProjetoMetaMensagem.Dominio.Help.Error;
using Microsoft.Extensions.Logging;
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
