using ProjetoMetaMensagem.Dominio.Help.Error;
using Microsoft.Extensions.Logging;
using ProjetoMetaMensagem.Dominio.Common;
using ProjetoMetaMensagem.Dominio.Interfaces.Mediator;
using ProjetoMetaMensagem.Dominio.Interfaces.Repositorios;

namespace ProjetoMetaMensagem.Dominio.UseCases.Pipeline.Deleta
{
    public class DeletaPipelineHandler : IRequestHandler<DeletaPipelineCommand, Response<bool>>
    {
        private readonly IPipelineRepository _repository;
        private readonly ILogger<DeletaPipelineHandler> _logger;

        public DeletaPipelineHandler(IPipelineRepository repository, ILogger<DeletaPipelineHandler> logger)
        {
            _repository = repository;
            _logger = logger;
        }

        public async Task<Response<bool>> Handle(DeletaPipelineCommand command)
        {
            var response = new Response<bool>();
            try
            {
                var linhasAfetadas = await _repository.Excluir(command.Id, command.EmpresaIdSolicitante);

                // Zero linhas significa que o pipeline nao existe OU pertence a outra empresa.
                // As duas situacoes devolvem a mesma mensagem de proposito: dizer "existe, mas
                // nao e seu" ja entregaria ao atacante que aquele id e valido.
                if (linhasAfetadas == 0)
                {
                    response.AddErro("Pipeline não encontrado.");
                    return response;
                }

                response.AddValue(true);
            }
            catch (Exception ex)
            {
                response.AddErroServico(ex, _logger, nameof(DeletaPipelineHandler));
            }
            return response;
        }
    }
}
