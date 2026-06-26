using ProjetoMetaMensagem.Dominio.Common;
using ProjetoMetaMensagem.Dominio.Interfaces.Mediator;
using ProjetoMetaMensagem.Dominio.Interfaces.Repositorios;

namespace ProjetoMetaMensagem.Dominio.UseCases.PipelineEtapa.Deleta
{
    public class DeletaEtapaHandler : IRequestHandler<DeletaEtapaCommand, Response<bool>>
    {
        private readonly IPipelineRepository _repository;
        public DeletaEtapaHandler(IPipelineRepository repository) => _repository = repository;

        public async Task<Response<bool>> Handle(DeletaEtapaCommand command)
        {
            var response = new Response<bool>();
            try
            {
                var totalLeads = await _repository.ContarLeadsPorEtapa(command.Id);
                if (totalLeads > 0)
                {
                    response.AddErro("Não é possível excluir uma etapa com leads. Remova os leads primeiro.");
                    return response;
                }
                await _repository.ExcluirEtapa(command.Id);
                response.AddValue(true);
            }
            catch (Exception ex)
            {
                response.AddErro($"Erro ao deletar etapa: {ex.Message}");
            }
            return response;
        }
    }
}
