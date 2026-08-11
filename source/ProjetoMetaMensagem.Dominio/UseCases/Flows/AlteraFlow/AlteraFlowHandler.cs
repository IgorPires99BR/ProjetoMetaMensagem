using ProjetoMetaMensagem.Dominio.Help.Error;
using Microsoft.Extensions.Logging;
using ProjetoMetaMensagem.Dominio.Common;
using ProjetoMetaMensagem.Dominio.Entidades;
using ProjetoMetaMensagem.Dominio.Interfaces;
using ProjetoMetaMensagem.Dominio.Interfaces.Mediator;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjetoMetaMensagem.Dominio.UseCases.Flows.AlteraFlow
{
    public class AlteraFlowHandler : IRequestHandler<AlteraFlowCommand, Response<AlteraFlowResult>>
    {
        private readonly IUnitOfWork _unitOfWork;

        private readonly ILogger<AlteraFlowHandler> _logger;

        public AlteraFlowHandler(IUnitOfWork unitOfWork, ILogger<AlteraFlowHandler> logger)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async Task<Response<AlteraFlowResult>> Handle(AlteraFlowCommand command)
        {
            var response = new Response<AlteraFlowResult>();

            var validator = new AlteraFlowValidator();
            var validateResult = validator.Validate(command);
            if (!validateResult.IsValid)
            {
                response.AddErros(validateResult.Errors.ToCustomValidationFailure());
                return response;
            }

            // 1. Verifica se o Flow realmente existe no banco
            var flowExistente = await _unitOfWork.Flow.ObterPorId(command.Id);
            if (flowExistente == null)
            {
                response.AddErro("Fluxo de conversa não encontrado.");
                return response;
            }

            // Editar recria TODAS as etapas com Guids novos (passo 3 abaixo). Uma conversa em
            // andamento guarda o Guid da etapa ANTIGA em EtapaAtualId -- se deixar editar, ela
            // fica apontando pra uma etapa que nao existe mais assim que o cliente responder.
            var conversasDoFlow = await _unitOfWork.ConversationState.ObterPorFlow(flowExistente.Id);
            if (conversasDoFlow.Any(c => !c.Finalizado))
            {
                response.AddErro("Este fluxo tem conversas em andamento e não pode ser editado. Aguarde finalizarem antes de alterar as etapas.");
                return response;
            }

            // 2. Atualiza os dados do cabeçalho
            flowExistente.Nome = command.Nome;
            flowExistente.Descricao = command.Descricao;
            flowExistente.GatilhoInicial = command.GatilhoPalavraChave;
            flowExistente.Ativo = command.Ativo;
            flowExistente.NumeroId = command.NumeroId;

            // 3. Monta a nova lista de etapas encadeadas
            var novasEtapas = new List<FlowEtapa>();
            FlowEtapa etapaAnterior = null;
            var passosOrdenados = command.Etapas.OrderBy(e => e.Ordem).ToList();

            for (int i = 0; i < passosOrdenados.Count; i++)
            {
                var dto = passosOrdenados[i];

                var novaEtapa = new FlowEtapa
                {
                    // Mesmo bug do CriaFlowHandler: sem Id explicito, toda etapa nascia
                    // com Guid.Empty e a segunda etapa violava a PK ao salvar.
                    Id = Guid.NewGuid(),
                    FlowId = flowExistente.Id,
                    NomeEtapa = dto.TipoStep,
                    ConteudoLivre = dto.MensagemPergunta,
                    EhEtapaInicial = (i == 0),
                    GatilhoResposta = dto.TipoStep == "Capturar Input" ? "Qualquer_Resposta" : "Avancar",
                    TemplateId = dto.TemplateId
                };

                if (etapaAnterior != null)
                {
                    etapaAnterior.ProximaEtapaId = novaEtapa.Id;
                }

                novasEtapas.Add(novaEtapa);
                etapaAnterior = novaEtapa;
            }

            try
            {
                _unitOfWork.BeginTransaction();
                // 4. Executa as alterações no banco de dados dentro da mesma transação

                // Atualiza o pai
                var linhasAfetadas = await _unitOfWork.Flow.Alterar(flowExistente, command.EmpresaIdSolicitante);

                // Zero linhas: fluxo inexistente ou de outra empresa. Mesma mensagem nos dois
                // casos, pra nao confirmar ao atacante que o id existe.
                if (linhasAfetadas == 0)
                {
                    _unitOfWork.Rollback();
                    response.AddErro("Fluxo não encontrado.");
                    return response;
                }

                // Conversas ja finalizadas nao bloqueiam a edicao (checado acima), mas a LINHA
                // continua no banco apontando (FK) pra uma FlowEtapa que esta prestes a ser
                // apagada. Sem limpar isso, todo flow que ja rodou uma conversa - mesmo ja
                // encerrada - fica travado pra sempre: o DELETE das etapas abaixo comecava a
                // falhar com violacao de FK assim que o primeiro cliente terminava o fluxo.
                foreach (var conversaFinalizada in conversasDoFlow)
                {
                    await _unitOfWork.ConversationState.Excluir(conversaFinalizada.Id);
                }

                // Remove todas as etapas antigas do banco para limpar o Grafo anterior
                await _unitOfWork.Flow.ExcluirEtapasPorFlowId(flowExistente.Id, command.EmpresaIdSolicitante);

                // Insere a nova arvore de etapas atualizada -- de tras pra frente, mesmo motivo
                // do CriaFlowHandler (ProximaEtapaId e uma FK auto-referenciada em FlowEtapa)
                for (int i = novasEtapas.Count - 1; i >= 0; i--)
                {
                    await _unitOfWork.Flow.IncluirEtapa(novasEtapas[i]);
                }

                // 5. Integração com a Meta
                // Aqui você atualizaria o Flow correspondente na API Cloud do WhatsApp se necessário

                // 7. Retorna o resultado
                var result = new AlteraFlowResult();

                response.AddValue(result);
                _unitOfWork.Commit();
            }
            catch (Exception ex)
            {
                _unitOfWork.Rollback();
                // Qualquer quebra (no banco ou integração) faz o UnitOfWork reverter tudo, mantendo o estado anterior
                response.AddErroServico(ex, _logger, nameof(AlteraFlowHandler));
            }

            return response;
        }
    }
}


