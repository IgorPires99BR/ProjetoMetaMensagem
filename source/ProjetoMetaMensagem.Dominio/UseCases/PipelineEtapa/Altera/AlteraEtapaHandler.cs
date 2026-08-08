using ProjetoMetaMensagem.Dominio.Help.Error;
using Microsoft.Extensions.Logging;
using ProjetoMetaMensagem.Dominio.Common;
using ProjetoMetaMensagem.Dominio.Interfaces.Mediator;
using ProjetoMetaMensagem.Dominio.Interfaces.Repositorios;

namespace ProjetoMetaMensagem.Dominio.UseCases.PipelineEtapa.Altera
{
    public class AlteraEtapaHandler : IRequestHandler<AlteraEtapaCommand, Response<AlteraEtapaResult>>
    {
        private readonly IPipelineRepository _repository;
        private readonly ILogger<AlteraEtapaHandler> _logger;

        public AlteraEtapaHandler(IPipelineRepository repository, ILogger<AlteraEtapaHandler> logger)
        {
            _repository = repository;
            _logger = logger;
        }

        public async Task<Response<AlteraEtapaResult>> Handle(AlteraEtapaCommand command)
        {
            var response = new Response<AlteraEtapaResult>();
            try
            {
                var etapa = await _repository.ObterEtapaPorId(command.Id);
                if (etapa == null)
                {
                    response.AddErro("Etapa não encontrada.");
                    return response;
                }
                etapa.Nome = command.Nome;
                etapa.Ordem = command.Ordem;
                etapa.Cor = command.Cor;
                etapa.DispararAoEntrar = command.DispararAoEntrar;
                etapa.TemplateIdAoEntrar = command.TemplateIdAoEntrar;
                var linhasAfetadas = await _repository.AlterarEtapa(etapa, command.EmpresaIdSolicitante);

                // Zero linhas: etapa inexistente ou de outra empresa. Mesma mensagem nos dois
                // casos, pra nao confirmar ao atacante que o id existe.
                if (linhasAfetadas == 0)
                {
                    response.AddErro("Etapa não encontrada.");
                    return response;
                }

                response.AddValue(new AlteraEtapaResult { Id = etapa.Id, Nome = etapa.Nome });
            }
            catch (Exception ex)
            {
                response.AddErro(TratamentoErro.Tratar(ex, _logger, nameof(AlteraEtapaHandler)));
            }
            return response;
        }
    }
}
