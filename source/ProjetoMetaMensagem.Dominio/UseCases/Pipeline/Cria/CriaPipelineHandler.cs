using ProjetoMetaMensagem.Dominio.Common;
using ProjetoMetaMensagem.Dominio.Interfaces.Mediator;
using ProjetoMetaMensagem.Dominio.Interfaces.Repositorios;

namespace ProjetoMetaMensagem.Dominio.UseCases.Pipeline.Cria
{
    public class CriaPipelineHandler : IRequestHandler<CriaPipelineCommand, Response<CriaPipelineResult>>
    {
        private readonly IPipelineRepository _repository;
        public CriaPipelineHandler(IPipelineRepository repository) => _repository = repository;

        public async Task<Response<CriaPipelineResult>> Handle(CriaPipelineCommand command)
        {
            var response = new Response<CriaPipelineResult>();
            try
            {
                var entity = new Entidades.Pipeline
                {
                    EmpresaId = command.EmpresaId,
                    Nome = command.Nome
                };
                await _repository.Incluir(entity);
                response.AddValue(new CriaPipelineResult { Id = entity.Id, Nome = entity.Nome });
            }
            catch (Exception ex)
            {
                response.AddErro($"Erro ao criar pipeline: {ex.Message}");
            }
            return response;
        }
    }
}
