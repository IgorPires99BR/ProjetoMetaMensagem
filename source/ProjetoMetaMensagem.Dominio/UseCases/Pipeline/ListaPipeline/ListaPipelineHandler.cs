using ProjetoMetaMensagem.Dominio.Help.Error;
using Microsoft.Extensions.Logging;
using ProjetoMetaMensagem.Dominio.Common;
using ProjetoMetaMensagem.Dominio.Interfaces.Mediator;
using ProjetoMetaMensagem.Dominio.Interfaces.Repositorios;

namespace ProjetoMetaMensagem.Dominio.UseCases.Pipeline.ListaPipeline
{
    public class ListaPipelineHandler : IRequestHandler<ListaPipelineCommand, Response<List<ListaPipelineResult>>>
    {
        private readonly IPipelineRepository _repository;
        private readonly ILogger<ListaPipelineHandler> _logger;

        public ListaPipelineHandler(IPipelineRepository repository, ILogger<ListaPipelineHandler> logger)
        {
            _repository = repository;
            _logger = logger;
        }

        public async Task<Response<List<ListaPipelineResult>>> Handle(ListaPipelineCommand command)
        {
            var response = new Response<List<ListaPipelineResult>>();

            var validator = new ListaPipelineValidator();
            var validateResult = validator.Validate(command);
            if (!validateResult.IsValid)
            {
                response.AddErros(validateResult.Errors.ToCustomValidationFailure());
                return response;
            }

            try
            {
                var pipelines = await _repository.ListarPorEmpresa(command.EmpresaId);
                var results = new List<ListaPipelineResult>();
                foreach (var p in pipelines)
                {
                    var etapas = await _repository.ListarEtapas(p.Id);
                    var totalLeads = 0;
                    foreach (var e in etapas)
                        totalLeads += await _repository.ContarLeadsPorEtapa(e.Id, command.EmpresaId);

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
                response.AddErroServico(ex, _logger, nameof(ListaPipelineHandler));
            }
            return response;
        }
    }
}
