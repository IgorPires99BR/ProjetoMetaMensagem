using ProjetoMetaMensagem.Dominio.Help.Error;
using Microsoft.Extensions.Logging;
using ProjetoMetaMensagem.Dominio.Common;
using ProjetoMetaMensagem.Dominio.Interfaces.Mediator;
using ProjetoMetaMensagem.Dominio.Interfaces.Repositorios;

namespace ProjetoMetaMensagem.Dominio.UseCases.Pipeline.DeletaPipeline
{
    public class DeletaPipelineHandler : IRequestHandler<DeletaPipelineCommand, Response<bool>>
    {
        private readonly IPipelineRepository _repository;
        private readonly ILogger<DeletaPipelineHandler> _logger;

        public DeletaPipelineHandler(IPipelineRepository repository, ILogger<DeletaPipelineHandler> logger)
        {
            _repository = repository;
            _logger = logger;
        }

        public async Task<Response<bool>> Handle(DeletaPipelineCommand command)
        {
            var response = new Response<bool>();

            var validator = new DeletaPipelineValidator();
            var validateResult = validator.Validate(command);
            if (!validateResult.IsValid)
            {
                response.AddErros(validateResult.Errors.ToCustomValidationFailure());
                return response;
            }

            try
            {
                var linhasAfetadas = await _repository.Excluir(command.Id, command.EmpresaIdSolicitante);

                // Zero linhas: pipeline inexistente ou de outra empresa. Mesma mensagem nos dois
                // casos, pra nao confirmar ao atacante que o id existe.
                if (linhasAfetadas == 0)
                {
                    response.AddErro("Pipeline não encontrado.");
                    return response;
                }

                response.AddValue(true);
            }
            catch (Exception ex)
            {
                response.AddErroServico(ex, _logger, nameof(DeletaPipelineHandler));
            }
            return response;
        }
    }
}
