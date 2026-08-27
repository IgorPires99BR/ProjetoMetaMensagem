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

            // Editar preserva o Id das etapas que ja existiam, entao conversa em andamento
            // continua valendo -- ela aponta pro Id da etapa atual, e esse Id nao muda mais.
            //
            // Antes a edicao apagava e recriava tudo com Ids novos, e por isso qualquer
            // conversa aberta bloqueava a edicao inteira. Na pratica isso significou encerrar
            // 22 conversas de leads a forca so pra poder mexer no texto de uma etapa.
            var conversasDoFlow = await _unitOfWork.ConversationState.ObterPorFlow(flowExistente.Id);
            var etapasNoBanco = await _unitOfWork.Flow.ObterEtapasPorFlow(flowExistente.Id);

            // O unico caso que ainda quebra conversa em andamento e REMOVER a etapa onde ela
            // esta parada. Bloqueia so isso, com o motivo especifico, em vez de proibir toda
            // edicao por precaucao.
            var idsQueVaoFicar = command.Etapas
                .Where(e => e.Id.HasValue)
                .Select(e => e.Id!.Value)
                .ToHashSet();

            var conversaOrfa = conversasDoFlow.FirstOrDefault(c =>
                !c.Finalizado &&
                c.EtapaAtualId.HasValue &&
                etapasNoBanco.Any(e => e.Id == c.EtapaAtualId.Value) &&
                !idsQueVaoFicar.Contains(c.EtapaAtualId.Value));

            if (conversaOrfa != null)
            {
                response.AddErro(
                    "Há uma conversa em andamento parada exatamente na etapa que você está removendo. " +
                    "Remova essa etapa depois que a conversa terminar, ou assuma a conversa pelo Chat antes.");
                return response;
            }

            // 2. Atualiza os dados do cabeçalho
            flowExistente.Nome = command.Nome;
            flowExistente.Descricao = command.Descricao;
            flowExistente.GatilhoInicial = command.GatilhoPalavraChave;
            flowExistente.Ativo = command.Ativo;
            flowExistente.NumeroId = command.NumeroId;
            flowExistente.SourceIdAnuncio = command.SourceIdAnuncio;

            // 3. Monta a nova lista de etapas encadeadas
            var novasEtapas = new List<FlowEtapa>();
            FlowEtapa etapaAnterior = null;
            var passosOrdenados = command.Etapas.OrderBy(e => e.Ordem).ToList();

            for (int i = 0; i < passosOrdenados.Count; i++)
            {
                var dto = passosOrdenados[i];

                // Reaproveita o Id de quem ja existe no banco; so etapa realmente nova ganha
                // Guid novo. E o que mantem valido o ponteiro das conversas em andamento.
                var jaExiste = dto.Id.HasValue && etapasNoBanco.Any(e => e.Id == dto.Id.Value);

                var novaEtapa = new FlowEtapa
                {
                    // Sem Id explicito toda etapa nasceria com Guid.Empty e a segunda violaria
                    // a PK ao salvar (bug historico, mesmo do CriaFlowHandler).
                    Id = jaExiste ? dto.Id!.Value : Guid.NewGuid(),
                    FlowId = flowExistente.Id,
                    NomeEtapa = dto.TipoStep,
                    ConteudoLivre = dto.MensagemPergunta,
                    EhEtapaInicial = (i == 0),
                    GatilhoResposta = dto.TipoStep == "Capturar Input" ? "Qualquer_Resposta" : "Avancar",
                    // A tela sempre pediu a variavel de saida e o valor chegava aqui, mas nao era
                    // gravado -- entao nenhuma etapa de captura guardava a resposta do cliente.
                    VariavelSaida = dto.VariavelSaida,
                    TemplateId = dto.TemplateId,
                    Botao1 = dto.Botao1,
                    Botao2 = dto.Botao2
                };

                if (etapaAnterior != null)
                {
                    etapaAnterior.ProximaEtapaId = novaEtapa.Id;
                }

                novasEtapas.Add(novaEtapa);
                etapaAnterior = novaEtapa;
            }

            // Resolve a ramificacao depois de todas as etapas existirem: OrdemDestinoB aponta
            // pra uma etapa que pode vir DEPOIS na lista, entao o Id dela so existe agora.
            for (int i = 0; i < passosOrdenados.Count; i++)
            {
                var destinoB = passosOrdenados[i].OrdemDestinoB;
                if (destinoB == null) continue;

                var indiceDestino = passosOrdenados.FindIndex(e => e.Ordem == destinoB.Value);
                if (indiceDestino < 0)
                {
                    response.AddErro($"A etapa {passosOrdenados[i].Ordem} aponta o segundo caminho para a etapa {destinoB.Value}, que não existe neste fluxo.");
                    return response;
                }

                novasEtapas[i].ProximaEtapaIdB = novasEtapas[indiceDestino].Id;
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

                // A gravacao acontece em tres passos por causa da FK auto-referenciada
                // (ProximaEtapaId aponta pra outra FluxoEtapa): so da pra apontar pra uma
                // etapa que ja exista.
                var idsNoBanco = etapasNoBanco.Select(e => e.Id).ToHashSet();

                // 1) Cria as etapas novas SEM ponteiro. O destino delas pode ser outra etapa
                //    nova que ainda nao existe -- gravar o ponteiro agora violaria a FK.
                foreach (var etapa in novasEtapas.Where(e => !idsNoBanco.Contains(e.Id)))
                {
                    var proximaId = etapa.ProximaEtapaId;
                    var proximaIdB = etapa.ProximaEtapaIdB;

                    etapa.ProximaEtapaId = null;
                    etapa.ProximaEtapaIdB = null;
                    await _unitOfWork.Flow.IncluirEtapa(etapa);

                    etapa.ProximaEtapaId = proximaId;
                    etapa.ProximaEtapaIdB = proximaIdB;
                }

                // 2) Agora que todas existem, grava conteudo e ponteiros de todas -- inclusive
                //    das que acabaram de ser criadas.
                foreach (var etapa in novasEtapas)
                {
                    await _unitOfWork.Flow.AlterarEtapa(etapa);
                }

                // 3) Remove o que o usuario tirou do flow. Vem por ultimo: no passo 2 ninguem
                //    mais aponta pra elas, entao a FK nao barra a exclusao.
                var idsQueFicaram = novasEtapas.Select(e => e.Id).ToHashSet();
                foreach (var removida in etapasNoBanco.Where(e => !idsQueFicaram.Contains(e.Id)))
                {
                    // Conversa ja encerrada continua apontando (FK) pra etapa removida. A
                    // conversa em andamento nessa etapa ja foi barrada la em cima.
                    foreach (var conversa in conversasDoFlow.Where(c => c.EtapaAtualId == removida.Id))
                    {
                        await _unitOfWork.ConversationState.Excluir(conversa.Id);
                    }

                    await _unitOfWork.Flow.ExcluirEtapa(removida.Id);
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


