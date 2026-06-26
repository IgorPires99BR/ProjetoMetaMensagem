using ProjetoMetaMensagem.Dominio.Common;
using ProjetoMetaMensagem.Dominio.Interfaces.Mediator;
using ProjetoMetaMensagem.Dominio.Interfaces.Repositorios;

namespace ProjetoMetaMensagem.Dominio.UseCases.LeadPipeline.Mover
{
    public class MoverLeadHandler : IRequestHandler<MoverLeadCommand, Response<MoverLeadResult>>
    {
        private readonly IPipelineRepository _repository;
        public MoverLeadHandler(IPipelineRepository repository) => _repository = repository;

        public async Task<Response<MoverLeadResult>> Handle(MoverLeadCommand command)
        {
            var response = new Response<MoverLeadResult>();
            try
            {
                await _repository.MoverLead(command.LeadId, command.NovaEtapaId);
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
                response.AddErro($"Erro ao mover lead: {ex.Message}");
            }
            return response;
        }
    }

    public class AdicionarLeadHandler : IRequestHandler<AdicionarLeadCommand, Response<MoverLeadResult>>
    {
        private readonly IPipelineRepository _repository;
        public AdicionarLeadHandler(IPipelineRepository repository) => _repository = repository;

        public async Task<Response<MoverLeadResult>> Handle(AdicionarLeadCommand command)
        {
            var response = new Response<MoverLeadResult>();
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
                response.AddValue(new MoverLeadResult
                {
                    Id = entity.Id,
                    NovaEtapaId = entity.PipelineEtapaId,
                    DataUltimaAlteracao = entity.DataUltimaAlteracao
                });
            }
            catch (Exception ex)
            {
                response.AddErro($"Erro ao adicionar lead: {ex.Message}");
            }
            return response;
        }
    }

    public class RemoverLeadHandler : IRequestHandler<RemoverLeadCommand, Response<bool>>
    {
        private readonly IPipelineRepository _repository;
        public RemoverLeadHandler(IPipelineRepository repository) => _repository = repository;

        public async Task<Response<bool>> Handle(RemoverLeadCommand command)
        {
            var response = new Response<bool>();
            try
            {
                await _repository.RemoverLead(command.LeadId);
                response.AddValue(true);
            }
            catch (Exception ex)
            {
                response.AddErro($"Erro ao remover lead: {ex.Message}");
            }
            return response;
        }
    }
}
