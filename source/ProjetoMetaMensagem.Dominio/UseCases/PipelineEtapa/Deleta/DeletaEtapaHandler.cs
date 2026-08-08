using ProjetoMetaMensagem.Dominio.Help.Error;
using Microsoft.Extensions.Logging;
using ProjetoMetaMensagem.Dominio.Common;
using ProjetoMetaMensagem.Dominio.Interfaces.Mediator;
using ProjetoMetaMensagem.Dominio.Interfaces.Repositorios;

namespace ProjetoMetaMensagem.Dominio.UseCases.PipelineEtapa.Deleta
{
    public class DeletaEtapaHandler : IRequestHandler<DeletaEtapaCommand, Response<bool>>
    {
        private readonly IPipelineRepository _repository;
        private readonly ILogger<DeletaEtapaHandler> _logger;

        public DeletaEtapaHandler(IPipelineRepository repository, ILogger<DeletaEtapaHandler> logger)
        {
            _repository = repository;
            _logger = logger;
        }

        public async Task<Response<bool>> Handle(DeletaEtapaCommand command)
        {
            var response = new Response<bool>();
            try
            {
                var totalLeads = await _repository.ContarLeadsPorEtapa(command.Id, command.EmpresaIdSolicitante);
                if (totalLeads > 0)
                {
                    response.AddErro("Não é possível excluir uma etapa com leads. Remova os leads primeiro.");
                    return response;
                }
                var linhasAfetadas = await _repository.ExcluirEtapa(command.Id, command.EmpresaIdSolicitante);

                // Zero linhas significa que a etapa nao existe OU pertence a outra empresa.
                // As duas situacoes devolvem a mesma mensagem de proposito: dizer "existe, mas
                // nao e sua" ja entregaria ao atacante que aquele id e valido.
                if (linhasAfetadas == 0)
                {
                    response.AddErro("Etapa não encontrada.");
                    return response;
                }

                response.AddValue(true);
            }
            catch (Exception ex)
            {
                response.AddErro(TratamentoErro.Tratar(ex, _logger, nameof(DeletaEtapaHandler)));
            }
            return response;
        }
    }
}
