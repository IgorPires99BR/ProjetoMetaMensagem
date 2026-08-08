using ProjetoMetaMensagem.Dominio.Help.Error;
using Microsoft.Extensions.Logging;
using ProjetoMetaMensagem.Dominio.Common;
using ProjetoMetaMensagem.Dominio.Interfaces.Mediator;
using ProjetoMetaMensagem.Dominio.Interfaces.Repositorios;

namespace ProjetoMetaMensagem.Dominio.UseCases.PipelineEtapa.Lista
{
    public class ListaEtapaHandler : IRequestHandler<ListaEtapaCommand, Response<List<ListaEtapaResult>>>
    {
        private readonly IPipelineRepository _repository;
        private readonly ILogger<ListaEtapaHandler> _logger;

        public ListaEtapaHandler(IPipelineRepository repository, ILogger<ListaEtapaHandler> logger)
        {
            _repository = repository;
            _logger = logger;
        }

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
                        // null: o comando so carrega o PipelineId, nao ha empresa a aplicar aqui.
                        // E contagem de exibicao, nao a operacao destrutiva que este recorte protege.
                        TotalLeads = await _repository.ContarLeadsPorEtapa(e.Id, null)
                    });
                }
                response.AddValue(results);
            }
            catch (Exception ex)
            {
                response.AddErro(TratamentoErro.Tratar(ex, _logger, nameof(ListaEtapaHandler)));
            }
            return response;
        }
    }
}
