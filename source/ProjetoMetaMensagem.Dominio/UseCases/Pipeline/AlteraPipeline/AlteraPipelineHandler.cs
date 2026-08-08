using ProjetoMetaMensagem.Dominio.Help.Error;
using Microsoft.Extensions.Logging;
using ProjetoMetaMensagem.Dominio.Common;
using ProjetoMetaMensagem.Dominio.Interfaces.Mediator;
using ProjetoMetaMensagem.Dominio.Interfaces.Repositorios;

namespace ProjetoMetaMensagem.Dominio.UseCases.Pipeline.AlteraPipeline
{
    public class AlteraPipelineHandler : IRequestHandler<AlteraPipelineCommand, Response<AlteraPipelineResult>>
    {
        private readonly IPipelineRepository _repository;
        private readonly ILogger<AlteraPipelineHandler> _logger;

        public AlteraPipelineHandler(IPipelineRepository repository, ILogger<AlteraPipelineHandler> logger)
        {
            _repository = repository;
            _logger = logger;
        }

        public async Task<Response<AlteraPipelineResult>> Handle(AlteraPipelineCommand command)
        {
            var response = new Response<AlteraPipelineResult>();

            var validator = new AlteraPipelineValidator();
            var validateResult = validator.Validate(command);
            if (!validateResult.IsValid)
            {
                response.AddErros(validateResult.Errors.ToCustomValidationFailure());
                return response;
            }

            try
            {
                var pipeline = await _repository.ObterPorId(command.Id);
                if (pipeline == null)
                {
                    response.AddErro("Pipeline não encontrado.");
                    return response;
                }
                pipeline.Nome = command.Nome;
                var linhasAfetadas = await _repository.Alterar(pipeline, command.EmpresaIdSolicitante);

                // Zero linhas: pipeline inexistente ou de outra empresa. Mesma mensagem nos dois
                // casos, pra nao confirmar ao atacante que o id existe.
                if (linhasAfetadas == 0)
                {
                    response.AddErro("Pipeline não encontrado.");
                    return response;
                }

                response.AddValue(new AlteraPipelineResult { Id = pipeline.Id, Nome = pipeline.Nome });
            }
            catch (Exception ex)
            {
                response.AddErroServico(ex, _logger, nameof(AlteraPipelineHandler));
            }
            return response;
        }
    }
}
