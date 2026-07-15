using ProjetoMetaMensagem.Dominio.Common;
using ProjetoMetaMensagem.Dominio.Interfaces.Mediator;
using ProjetoMetaMensagem.Dominio.Interfaces.Repositorios;

namespace ProjetoMetaMensagem.Dominio.UseCases.PipelineEtapa.Lista
{
    public class ListaEtapaHandler : IRequestHandler<ListaEtapaCommand, Response<List<ListaEtapaResult>>>
    {
        private readonly IPipelineRepository _repository;
        public ListaEtapaHandler(IPipelineRepository repository) => _repository = repository;

        public async Task<Response<List<ListaEtapaResult>>> Handle(ListaEtapaCommand command)
        {
            var response = new Response<List<ListaEtapaResult>>();
            try
            {
                var etapas = await _repository.ListarEtapas(command.PipelineId);
                var results = new List<ListaEtapaResult>();
                foreach (var e in etapas.OrderBy(e => e.Ordem))
                {
                    results.Add(new ListaEtapaResult
                    {
                        Id = e.Id,
                        PipelineId = e.PipelineId,
                        Nome = e.Nome,
                        Ordem = e.Ordem,
                        Cor = e.Cor,
                        DispararAoEntrar = e.DispararAoEntrar,
                        TemplateIdAoEntrar = e.TemplateIdAoEntrar,
                        TotalLeads = await _repository.ContarLeadsPorEtapa(e.Id)
                    });
                }
                response.AddValue(results);
            }
            catch (Exception ex)
            {
                response.AddErro($"Erro ao listar etapas: {ex.Message}");
            }
            return response;
        }
    }
}
