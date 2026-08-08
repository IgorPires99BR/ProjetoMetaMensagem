using Microsoft.Extensions.Logging;
using ProjetoMetaMensagem.Dominio.Common;
using ProjetoMetaMensagem.Dominio.Help.Error;
using ProjetoMetaMensagem.Dominio.Interfaces.Mediator;
using ProjetoMetaMensagem.Dominio.Interfaces.Repositorios;

namespace ProjetoMetaMensagem.Dominio.UseCases.LeadPipeline.AdicionarLead
{
    public class AdicionarLeadHandler : IRequestHandler<AdicionarLeadCommand, Response<AdicionarLeadResult>>
    {
        private readonly IPipelineRepository _repository;
        private readonly ILogger<AdicionarLeadHandler> _logger;

        public AdicionarLeadHandler(IPipelineRepository repository, ILogger<AdicionarLeadHandler> logger)
        {
            _repository = repository;
            _logger = logger;
        }

        public async Task<Response<AdicionarLeadResult>> Handle(AdicionarLeadCommand command)
        {
            var response = new Response<AdicionarLeadResult>();

            var validator = new AdicionarLeadValidator();
            var validateResult = validator.Validate(command);
            if (!validateResult.IsValid)
            {
                response.AddErros(validateResult.Errors.ToCustomValidationFailure());
                return response;
            }

            try
            {
                var existe = await _repository.LeadJaExiste(command.EmpresaId, command.ContatoId);
                if (existe)
                {
                    response.AddErro("Este contato já está em um pipeline.");
                    return response;
                }

                var entity = new Entidades.LeadPipeline
                {
                    EmpresaId = command.EmpresaId,
                    ContatoId = command.ContatoId,
                    PipelineEtapaId = command.PipelineEtapaId,
                    Valor = command.Valor,
                    Observacao = command.Observacao
                };
                await _repository.IncluirLead(entity);
                response.AddValue(new AdicionarLeadResult
                {
                    Id = entity.Id,
                    NovaEtapaId = entity.PipelineEtapaId,
                    DataUltimaAlteracao = entity.DataUltimaAlteracao
                });
            }
            catch (Exception ex)
            {
                response.AddErroServico(ex, _logger, nameof(AdicionarLeadHandler));
            }
            return response;
        }
    }
}
