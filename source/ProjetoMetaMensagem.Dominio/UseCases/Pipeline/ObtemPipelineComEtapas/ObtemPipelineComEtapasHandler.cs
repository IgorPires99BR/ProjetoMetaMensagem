using ProjetoMetaMensagem.Dominio.Help.Error;
using Microsoft.Extensions.Logging;
using ProjetoMetaMensagem.Dominio.Common;
using ProjetoMetaMensagem.Dominio.Interfaces.Mediator;
using ProjetoMetaMensagem.Dominio.Interfaces.Repositorios;

namespace ProjetoMetaMensagem.Dominio.UseCases.Pipeline.ObtemPipelineComEtapas
{
    public class ObtemPipelineComEtapasHandler : IRequestHandler<ObtemPipelineComEtapasCommand, Response<ObtemPipelineComEtapasResult>>
    {
        private readonly IPipelineRepository _repository;
        private readonly ILogger<ObtemPipelineComEtapasHandler> _logger;

        public ObtemPipelineComEtapasHandler(IPipelineRepository repository, ILogger<ObtemPipelineComEtapasHandler> logger)
        {
            _repository = repository;
            _logger = logger;
        }

        public async Task<Response<ObtemPipelineComEtapasResult>> Handle(ObtemPipelineComEtapasCommand command)
        {
            var response = new Response<ObtemPipelineComEtapasResult>();

            var validator = new ObtemPipelineComEtapasValidator();
            var validateResult = validator.Validate(command);
            if (!validateResult.IsValid)
            {
                response.AddErros(validateResult.Errors.ToCustomValidationFailure());
                return response;
            }

            try
            {
                var pipeline = await _repository.ObterPorId(command.PipelineId);
                // PipelineId nao vem no filtro global (so olha nomes tipo "empresaId"), entao sem
                // esta checagem um EmpresaId valido (o proprio, ja verificado pelo filtro) combinado
                // com um PipelineId de OUTRA empresa devolvia a estrutura do funil alheio.
                if (pipeline == null || pipeline.EmpresaId != command.EmpresaId)
                {
                    response.AddErro("Pipeline não encontrado.");
                    return response;
                }

                var etapas = await _repository.ListarEtapas(command.PipelineId);
                var leads = await _repository.ListarLeads(command.EmpresaId);

                var result = new ObtemPipelineComEtapasResult
                {
                    Id = pipeline.Id,
                    Nome = pipeline.Nome,
                    Etapas = etapas.OrderBy(e => e.Ordem).Select(etapa => new EtapaComLeads
                    {
                        Id = etapa.Id,
                        Nome = etapa.Nome,
                        Ordem = etapa.Ordem,
                        Cor = etapa.Cor,
                        DispararAoEntrar = etapa.DispararAoEntrar,
                        TemplateIdAoEntrar = etapa.TemplateIdAoEntrar,
                        Leads = leads.Where(l => l.PipelineEtapaId == etapa.Id).Select(l => new LeadNaEtapa
                        {
                            Id = l.Id,
                            ContatoId = l.ContatoId,
                            NomeContato = string.Empty,
                            Telefone = string.Empty,
                            Valor = l.Valor,
                            Observacao = l.Observacao,
                            DataEntrada = l.DataEntrada
                        }).ToList()
                    }).ToList()
                };

                response.AddValue(result);
            }
            catch (Exception ex)
            {
                response.AddErroServico(ex, _logger, nameof(ObtemPipelineComEtapasHandler));
            }
            return response;
        }
    }
}
