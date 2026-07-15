using ProjetoMetaMensagem.Dominio.Common;
using ProjetoMetaMensagem.Dominio.Interfaces.Mediator;
using ProjetoMetaMensagem.Dominio.Interfaces.Repositorios;

namespace ProjetoMetaMensagem.Dominio.UseCases.Pipeline.Altera
{
    public class AlteraPipelineHandler : IRequestHandler<AlteraPipelineCommand, Response<AlteraPipelineResult>>
    {
        private readonly IPipelineRepository _repository;
        public AlteraPipelineHandler(IPipelineRepository repository) => _repository = repository;

        public async Task<Response<AlteraPipelineResult>> Handle(AlteraPipelineCommand command)
        {
            var response = new Response<AlteraPipelineResult>();
            try
            {
                var pipeline = await _repository.ObterPorId(command.Id);
                if (pipeline == null)
                {
                    response.AddErro("Pipeline não encontrado.");
                    return response;
                }
                pipeline.Nome = command.Nome;
                await _repository.Alterar(pipeline);
                response.AddValue(new AlteraPipelineResult { Id = pipeline.Id, Nome = pipeline.Nome });
            }
            catch (Exception ex)
            {
                response.AddErro($"Erro ao alterar pipeline: {ex.Message}");
            }
            return response;
        }
    }
}
