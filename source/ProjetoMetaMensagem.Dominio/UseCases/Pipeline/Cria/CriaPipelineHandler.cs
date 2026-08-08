using ProjetoMetaMensagem.Dominio.Help.Error;
using Microsoft.Extensions.Logging;
using ProjetoMetaMensagem.Dominio.Common;
using ProjetoMetaMensagem.Dominio.Interfaces.Mediator;
using ProjetoMetaMensagem.Dominio.Interfaces.Repositorios;

namespace ProjetoMetaMensagem.Dominio.UseCases.Pipeline.Cria
{
    public class CriaPipelineHandler : IRequestHandler<CriaPipelineCommand, Response<CriaPipelineResult>>
    {
        private readonly IPipelineRepository _repository;
        private readonly ILogger<CriaPipelineHandler> _logger;

        public CriaPipelineHandler(IPipelineRepository repository, ILogger<CriaPipelineHandler> logger)
        {
            _repository = repository;
            _logger = logger;
        }

        public async Task<Response<CriaPipelineResult>> Handle(CriaPipelineCommand command)
        {
            var response = new Response<CriaPipelineResult>();
            try
            {
                var entity = new Entidades.Pipeline
                {
                    EmpresaId = command.EmpresaId,
                    Nome = command.Nome
                };
                await _repository.Incluir(entity);
                response.AddValue(new CriaPipelineResult { Id = entity.Id, Nome = entity.Nome });
            }
            catch (Exception ex)
            {
                response.AddErro(TratamentoErro.Tratar(ex, _logger, nameof(CriaPipelineHandler)));
            }
            return response;
        }
    }
}
