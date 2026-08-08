using ProjetoMetaMensagem.Dominio.Help.Error;
using Microsoft.Extensions.Logging;
using ProjetoMetaMensagem.Dominio.Common;
using ProjetoMetaMensagem.Dominio.Interfaces.Mediator;
using ProjetoMetaMensagem.Dominio.Interfaces.Repositorios;

namespace ProjetoMetaMensagem.Dominio.UseCases.PipelineEtapa.Cria
{
    public class CriaEtapaHandler : IRequestHandler<CriaEtapaCommand, Response<CriaEtapaResult>>
    {
        private readonly IPipelineRepository _repository;
        private readonly ILogger<CriaEtapaHandler> _logger;

        public CriaEtapaHandler(IPipelineRepository repository, ILogger<CriaEtapaHandler> logger)
        {
            _repository = repository;
            _logger = logger;
        }

        public async Task<Response<CriaEtapaResult>> Handle(CriaEtapaCommand command)
        {
            var response = new Response<CriaEtapaResult>();
            try
            {
                var entity = new Entidades.PipelineEtapa
                {
                    PipelineId = command.PipelineId,
                    Nome = command.Nome,
                    Ordem = command.Ordem,
                    Cor = command.Cor,
                    DispararAoEntrar = command.DispararAoEntrar,
                    TemplateIdAoEntrar = command.TemplateIdAoEntrar
                };
                await _repository.IncluirEtapa(entity);
                response.AddValue(new CriaEtapaResult
                {
                    Id = entity.Id,
                    PipelineId = entity.PipelineId,
                    Nome = entity.Nome,
                    Ordem = entity.Ordem
                });
            }
            catch (Exception ex)
            {
                response.AddErroServico(ex, _logger, nameof(CriaEtapaHandler));
            }
            return response;
        }
    }
}
