using Microsoft.Extensions.Logging;
using ProjetoMetaMensagem.Dominio.Common;
using ProjetoMetaMensagem.Dominio.Help.Error;
using ProjetoMetaMensagem.Dominio.Interfaces.Mediator;
using ProjetoMetaMensagem.Dominio.Interfaces.Repositorios;

namespace ProjetoMetaMensagem.Dominio.UseCases.LeadPipeline.MoverLead
{
    public class MoverLeadHandler : IRequestHandler<MoverLeadCommand, Response<MoverLeadResult>>
    {
        private readonly IPipelineRepository _repository;
        private readonly ILogger<MoverLeadHandler> _logger;

        public MoverLeadHandler(IPipelineRepository repository, ILogger<MoverLeadHandler> logger)
        {
            _repository = repository;
            _logger = logger;
        }

        public async Task<Response<MoverLeadResult>> Handle(MoverLeadCommand command)
        {
            var response = new Response<MoverLeadResult>();

            var validator = new MoverLeadValidator();
            var validateResult = validator.Validate(command);
            if (!validateResult.IsValid)
            {
                response.AddErros(validateResult.Errors.ToCustomValidationFailure());
                return response;
            }

            try
            {
                var linhasAfetadas = await _repository.MoverLead(
                    command.LeadId, command.NovaEtapaId, command.EmpresaIdSolicitante);

                // Zero linhas: lead inexistente, de outra empresa, ou etapa de destino de outra
                // empresa. Mesma mensagem nos tres casos, pra nao confirmar ao atacante quais
                // ids existem.
                if (linhasAfetadas == 0)
                {
                    response.AddErro("Lead não encontrado.");
                    return response;
                }

                var lead = await _repository.ObterLead(command.LeadId);
                if (lead == null)
                {
                    response.AddErro("Lead não encontrado.");
                    return response;
                }
                response.AddValue(new MoverLeadResult
                {
                    Id = lead.Id,
                    NovaEtapaId = lead.PipelineEtapaId,
                    DataUltimaAlteracao = lead.DataUltimaAlteracao
                });
            }
            catch (Exception ex)
            {
                response.AddErroServico(ex, _logger, nameof(MoverLeadHandler));
            }
            return response;
        }
    }
}
