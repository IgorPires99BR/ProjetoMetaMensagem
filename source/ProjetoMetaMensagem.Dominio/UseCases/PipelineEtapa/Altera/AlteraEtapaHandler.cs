using ProjetoMetaMensagem.Dominio.Common;
using ProjetoMetaMensagem.Dominio.Interfaces.Mediator;
using ProjetoMetaMensagem.Dominio.Interfaces.Repositorios;

namespace ProjetoMetaMensagem.Dominio.UseCases.PipelineEtapa.Altera
{
    public class AlteraEtapaHandler : IRequestHandler<AlteraEtapaCommand, Response<AlteraEtapaResult>>
    {
        private readonly IPipelineRepository _repository;
        public AlteraEtapaHandler(IPipelineRepository repository) => _repository = repository;

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
                await _repository.AlterarEtapa(etapa);
                response.AddValue(new AlteraEtapaResult { Id = etapa.Id, Nome = etapa.Nome });
            }
            catch (Exception ex)
            {
                response.AddErro($"Erro ao alterar etapa: {ex.Message}");
            }
            return response;
        }
    }
}
