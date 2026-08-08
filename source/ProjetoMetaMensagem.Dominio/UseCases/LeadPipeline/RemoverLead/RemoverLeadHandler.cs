using Microsoft.Extensions.Logging;
using ProjetoMetaMensagem.Dominio.Common;
using ProjetoMetaMensagem.Dominio.Help.Error;
using ProjetoMetaMensagem.Dominio.Interfaces.Mediator;
using ProjetoMetaMensagem.Dominio.Interfaces.Repositorios;

namespace ProjetoMetaMensagem.Dominio.UseCases.LeadPipeline.RemoverLead
{
    public class RemoverLeadHandler : IRequestHandler<RemoverLeadCommand, Response<bool>>
    {
        private readonly IPipelineRepository _repository;
        private readonly ILogger<RemoverLeadHandler> _logger;

        public RemoverLeadHandler(IPipelineRepository repository, ILogger<RemoverLeadHandler> logger)
        {
            _repository = repository;
            _logger = logger;
        }

        public async Task<Response<bool>> Handle(RemoverLeadCommand command)
        {
            var response = new Response<bool>();

            var validator = new RemoverLeadValidator();
            var validateResult = validator.Validate(command);
            if (!validateResult.IsValid)
            {
                response.AddErros(validateResult.Errors.ToCustomValidationFailure());
                return response;
            }

            try
            {
                var linhasAfetadas = await _repository.RemoverLead(command.LeadId, command.EmpresaIdSolicitante);

                // Zero linhas significa que o lead nao existe OU pertence a outra empresa.
                // As duas situacoes devolvem a mesma mensagem de proposito: dizer "existe, mas
                // nao e seu" ja entregaria ao atacante que aquele id e valido.
                if (linhasAfetadas == 0)
                {
                    response.AddErro("Lead não encontrado.");
                    return response;
                }

                response.AddValue(true);
            }
            catch (Exception ex)
            {
                response.AddErroServico(ex, _logger, nameof(RemoverLeadHandler));
            }
            return response;
        }
    }
}
