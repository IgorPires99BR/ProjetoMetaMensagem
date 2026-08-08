using Microsoft.Extensions.Logging;
using ProjetoMetaMensagem.Dominio.Common;
using ProjetoMetaMensagem.Dominio.Help.Error;
using ProjetoMetaMensagem.Dominio.Interfaces.Mediator;
using ProjetoMetaMensagem.Dominio.Interfaces.Repositorios;

namespace ProjetoMetaMensagem.Dominio.UseCases.LeadPipeline.Mover
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
                response.AddErro(TratamentoErro.Tratar(ex, _logger, nameof(MoverLeadHandler)));
            }
            return response;
        }
    }

    public class AdicionarLeadHandler : IRequestHandler<AdicionarLeadCommand, Response<MoverLeadResult>>
    {
        private readonly IPipelineRepository _repository;
        private readonly ILogger<AdicionarLeadHandler> _logger;

        public AdicionarLeadHandler(IPipelineRepository repository, ILogger<AdicionarLeadHandler> logger)
        {
            _repository = repository;
            _logger = logger;
        }

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
                response.AddErro(TratamentoErro.Tratar(ex, _logger, nameof(AdicionarLeadHandler)));
            }
            return response;
        }
    }

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
                response.AddErro(TratamentoErro.Tratar(ex, _logger, nameof(RemoverLeadHandler)));
            }
            return response;
        }
    }
}
