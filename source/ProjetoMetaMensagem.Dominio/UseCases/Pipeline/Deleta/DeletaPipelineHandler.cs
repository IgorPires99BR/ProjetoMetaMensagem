using ProjetoMetaMensagem.Dominio.Common;
using ProjetoMetaMensagem.Dominio.Interfaces.Mediator;
using ProjetoMetaMensagem.Dominio.Interfaces.Repositorios;

namespace ProjetoMetaMensagem.Dominio.UseCases.Pipeline.Deleta
{
    public class DeletaPipelineHandler : IRequestHandler<DeletaPipelineCommand, Response<bool>>
    {
        private readonly IPipelineRepository _repository;
        public DeletaPipelineHandler(IPipelineRepository repository) => _repository = repository;

        public async Task<Response<bool>> Handle(DeletaPipelineCommand command)
        {
            var response = new Response<bool>();
            try
            {
                await _repository.Excluir(command.Id);
                response.AddValue(true);
            }
            catch (Exception ex)
            {
                response.AddErro($"Erro ao deletar pipeline: {ex.Message}");
            }
            return response;
        }
    }
}
