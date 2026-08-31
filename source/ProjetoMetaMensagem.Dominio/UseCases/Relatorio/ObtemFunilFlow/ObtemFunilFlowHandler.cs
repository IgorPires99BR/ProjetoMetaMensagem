using Microsoft.Extensions.Logging;
using ProjetoMetaMensagem.Dominio.Common;
using ProjetoMetaMensagem.Dominio.Entidades;
using ProjetoMetaMensagem.Dominio.Help.Error;
using ProjetoMetaMensagem.Dominio.Interfaces;
using ProjetoMetaMensagem.Dominio.Interfaces.Mediator;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ProjetoMetaMensagem.Dominio.UseCases.Relatorio.ObtemFunilFlow
{
    public class ObtemFunilFlowHandler : IRequestHandler<ObtemFunilFlowCommand, Response<ObtemFunilFlowResult>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<ObtemFunilFlowHandler> _logger;

        public ObtemFunilFlowHandler(IUnitOfWork unitOfWork, ILogger<ObtemFunilFlowHandler> logger)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async Task<Response<ObtemFunilFlowResult>> Handle(ObtemFunilFlowCommand command)
        {
            var response = new Response<ObtemFunilFlowResult>();

            var validateResult = new ObtemFunilFlowValidator().Validate(command);
            if (!validateResult.IsValid)
            {
                response.AddErros(validateResult.Errors.ToCustomValidationFailure());
                return response;
            }

            try
            {
                var flow = await _unitOfWork.Flow.ObterPorId(command.FlowId);
                if (flow == null)
                {
                    response.AddErro("Flow não encontrado.");
                    return response;
                }

                // Mesma regra do resto do dominio: quem nao e admin de plataforma so enxerga o
                // funil dos proprios flows. Sem isto, o id de um flow de outra empresa (visivel
                // em qualquer URL copiada) devolveria quantas pessoas essa empresa concorrente
                // tem presas em cada etapa.
                if (!command.SolicitanteEhAdmin && flow.EmpresaId != command.EmpresaIdSolicitante)
                {
                    response.AddErro("Você não tem permissão para ver o funil deste flow.");
                    return response;
                }

                var etapas = await _unitOfWork.Flow.ObterEtapasPorFlow(command.FlowId);
                var conversas = await _unitOfWork.ConversationState.ObterPorFlow(command.FlowId);

                var resultado = MontarFunil(flow.Id, flow.Nome, etapas, conversas);
                response.AddValue(resultado);
            }
            catch (Exception ex)
            {
                response.AddErroServico(ex, _logger, nameof(ObtemFunilFlowHandler));
            }

            return response;
        }

        // Separado do Handle para poder ser testado sem banco: monta os numeros a partir de
        // listas ja carregadas, do jeito que o teste de unidade e o handler os usam.
        public static ObtemFunilFlowResult MontarFunil(Guid flowId, string nomeFlow, List<FlowEtapa> etapas, List<ConversationState> conversas)
        {
            var ordem = OrdenarEtapas(etapas);

            var porEtapa = conversas
                .Where(c => c.EtapaAtualId.HasValue)
                .ToLookup(c => c.EtapaAtualId!.Value);

            var idsComEtapa = new HashSet<Guid>(etapas.Select(e => e.Id));
            var etapasFinais = new HashSet<Guid>(etapas
                .Where(e => e.ProximaEtapaId == null && e.ProximaEtapaIdB == null)
                .Select(e => e.Id));

            var dtoEtapas = ordem.Select((etapa, indice) =>
            {
                var conversasDaEtapa = porEtapa[etapa.Id].ToList();

                return new FunilEtapaDto
                {
                    EtapaId = etapa.Id,
                    Ordem = indice + 1,
                    NomeEtapa = etapa.NomeEtapa,
                    Rotulo = Truncar(etapa.ConteudoLivre, 80),
                    EhEtapaFinal = etapasFinais.Contains(etapa.Id),
                    Presas = conversasDaEtapa.Count(c => !c.Finalizado && !c.AguardandoAtendente),
                    EntreguesAoAtendente = conversasDaEtapa.Count(c => !c.Finalizado && c.AguardandoAtendente),
                    Concluiram = conversasDaEtapa.Count(c => c.Finalizado),
                };
            }).ToList();

            // Conversas cujo EtapaAtualId nao bate com nenhuma etapa deste flow (etapa excluida
            // depois que a conversa passou por ela) nao entram na lista por etapa, mas ainda
            // contam nos totais -- senao o total da tela nao bateria com o que apareceu detalhado.
            var orfas = conversas.Where(c => !c.EtapaAtualId.HasValue || !idsComEtapa.Contains(c.EtapaAtualId.Value)).ToList();

            return new ObtemFunilFlowResult
            {
                FlowId = flowId,
                NomeFlow = nomeFlow,
                TotalConversas = conversas.Count,
                TotalConcluiram = conversas.Count(c => c.Finalizado) ,
                TotalEntreguesAoAtendente = conversas.Count(c => !c.Finalizado && c.AguardandoAtendente),
                TotalPresas = conversas.Count(c => !c.Finalizado && !c.AguardandoAtendente),
                Etapas = dtoEtapas,
            };
        }

        // O banco nao guarda uma posicao explicita pra etapa -- so o encadeamento
        // (ProximaEtapaId / ProximaEtapaIdB) a partir da etapa marcada como inicial. Anda em
        // largura a partir dela: o passo principal (ProximaEtapaId) primeiro, o ramo B do botao
        // logo depois, na profundidade em que ele se abre. Mesma ideia que a tela de Flows usa
        // pra desenhar o fluxograma, so que aqui precisa cobrir os dois caminhos, nao so um.
        private static List<FlowEtapa> OrdenarEtapas(List<FlowEtapa> etapas)
        {
            var porId = etapas.ToDictionary(e => e.Id);
            var inicial = etapas.FirstOrDefault(e => e.EhEtapaInicial) ?? etapas.FirstOrDefault();

            var ordenadas = new List<FlowEtapa>();
            var visitadas = new HashSet<Guid>();
            var fila = new Queue<FlowEtapa>();

            if (inicial != null)
            {
                fila.Enqueue(inicial);
                visitadas.Add(inicial.Id);
            }

            while (fila.Count > 0)
            {
                var atual = fila.Dequeue();
                ordenadas.Add(atual);

                foreach (var proximoId in new[] { atual.ProximaEtapaId, atual.ProximaEtapaIdB })
                {
                    if (proximoId.HasValue && porId.TryGetValue(proximoId.Value, out var proxima) && visitadas.Add(proxima.Id))
                    {
                        fila.Enqueue(proxima);
                    }
                }
            }

            // Etapa orfa (sem ninguem apontando pra ela -- normalmente lixo de uma edicao
            // antiga) ainda entra na lista, no fim, pra nao sumir uma conversa presa nela.
            foreach (var etapa in etapas)
            {
                if (visitadas.Add(etapa.Id))
                {
                    ordenadas.Add(etapa);
                }
            }

            return ordenadas;
        }

        private static string? Truncar(string? texto, int tamanho)
        {
            if (string.IsNullOrWhiteSpace(texto)) return texto;
            return texto.Length <= tamanho ? texto : texto[..tamanho].TrimEnd() + "…";
        }
    }
}
