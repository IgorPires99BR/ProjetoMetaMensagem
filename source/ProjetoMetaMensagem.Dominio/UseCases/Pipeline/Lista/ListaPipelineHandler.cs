using ProjetoMetaMensagem.Dominio.Common;
using ProjetoMetaMensagem.Dominio.Interfaces.Mediator;
using ProjetoMetaMensagem.Dominio.Interfaces.Repositorios;

namespace ProjetoMetaMensagem.Dominio.UseCases.Pipeline.Lista
{
    public class ListaPipelineHandler : IRequestHandler<ListaPipelineCommand, Response<List<ListaPipelineResult>>>
    {
        private readonly IPipelineRepository _repository;
        public ListaPipelineHandler(IPipelineRepository repository) => _repository = repository;

        public async Task<Response<List<ListaPipelineResult>>> Handle(ListaPipelineCommand command)
        {
            var response = new Response<List<ListaPipelineResult>>();
            try
            {
                var pipelines = await _repository.ListarPorEmpresa(command.EmpresaId);
                var results = new List<ListaPipelineResult>();
                foreach (var p in pipelines)
                {
                    var etapas = await _repository.ListarEtapas(p.Id);
                    var totalLeads = 0;
                    foreach (var e in etapas)
                        totalLeads += await _repository.ContarLeadsPorEtapa(e.Id);

                    results.Add(new ListaPipelineResult
                    {
                        Id = p.Id,
                        EmpresaId = p.EmpresaId,
                        Nome = p.Nome,
                        DataCriacao = p.DataCriacao,
                        TotalEtapas = etapas.Count,
                        TotalLeads = totalLeads
                    });
                }
                response.AddValue(results);
            }
            catch (Exception ex)
            {
                response.AddErro($"Erro ao listar pipelines: {ex.Message}");
            }
            return response;
        }
    }
}
