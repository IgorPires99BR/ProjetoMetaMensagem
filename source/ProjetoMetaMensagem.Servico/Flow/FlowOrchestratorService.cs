using Newtonsoft.Json;
using ProjetoMetaMensagem.Dominio.Entidades;
using ProjetoMetaMensagem.Dominio.Interfaces;
using ProjetoMetaMensagem.Dominio.Interfaces.Repositorios;
using ProjetoMetaMensagem.Dominio.Interfaces.Servicos;
using System.Text.RegularExpressions;

namespace ProjetoMetaMensagem.Servico.Flow
{
    public class FlowOrchestratorService : IFlowOrchestratorService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMetaService _metaService;

        public FlowOrchestratorService(IUnitOfWork unitOfWork, IMetaService metaService)
        {
            _unitOfWork = unitOfWork;
            _metaService = metaService;
        }

        public async Task<FlowOrchestrationResult> ProcessarMensagem(Guid empresaId, Guid contatoId, string mensagem)
        {
            var resultado = new FlowOrchestrationResult();

            try
            {
                _unitOfWork.BeginTransaction();

                // 1. Verifica se ja existe uma conversa ativa para este contato
                var estadoAtual = await _unitOfWork.ConversationState.ObterPorEmpresaEContato(empresaId, contatoId);

                if (estadoAtual == null)
                {
                    // 2. Nao ha conversa ativa -> busca um flow cujo gatilho corresponda a mensagem
                    var flows = await _unitOfWork.Flow.ObterTodosPorEmpresa(empresaId);
                    var flowAtivado = flows.FirstOrDefault(f =>
                        f.Ativo &&
                        !string.IsNullOrEmpty(f.GatilhoInicial) &&
                        f.GatilhoInicial.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                            .Any(g => mensagem.Trim().Equals(g, StringComparison.OrdinalIgnoreCase)));

                    if (flowAtivado == null)
                    {
                        _unitOfWork.Commit();
                        resultado.Sucesso = false;
                        resultado.Mensagem = "Nenhum flow encontrado para esta mensagem.";
                        return resultado;
                    }

                    // 3. Obtem a etapa inicial do flow
                    var etapaInicial = await _unitOfWork.Flow.ObterEtapaInicial(flowAtivado.Id);
                    if (etapaInicial == null)
                    {
                        _unitOfWork.Commit();
                        resultado.Sucesso = false;
                        resultado.Mensagem = "Flow nao possui etapa inicial configurada.";
                        return resultado;
                    }

                    // 4. Cria novo estado de conversa
                    estadoAtual = new ConversationState
                    {
                        EmpresaId = empresaId,
                        ContatoId = contatoId,
                        FlowId = flowAtivado.Id,
                        EtapaAtualId = etapaInicial.Id,
                        Variaveis = "{}",
                        Finalizado = false
                    };

                    await _unitOfWork.ConversationState.Incluir(estadoAtual);

                    // 5. Executa a etapa inicial (deve ser uma mensagem de boas-vindas)
                    await ExecutarEtapa(etapaInicial, null, estadoAtual);

                    resultado.Sucesso = true;
                    resultado.FlowId = flowAtivado.Id;
                    resultado.EtapaId = etapaInicial.Id;
                    resultado.Mensagem = $"Flow '{flowAtivado.Nome}' iniciado.";
                }
                else
                {
                    // 6. Conversa ja ativa -> avanca para a proxima etapa baseado na resposta
                    var etapaAtual = await _unitOfWork.Flow.ObterEtapaPorId(estadoAtual.EtapaAtualId.Value);
                    if (etapaAtual == null)
                    {
                        _unitOfWork.Commit();
                        resultado.Sucesso = false;
                        resultado.Mensagem = "Etapa atual nao encontrada.";
                        return resultado;
                    }

                    // 7. Se a etapa atual captura input, salva a resposta como variavel
                    if (etapaAtual.NomeEtapa == "Capturar Input")
                    {
                        var variavelNome = ObterVariavelSaida(etapaAtual);
                        if (!string.IsNullOrEmpty(variavelNome))
                        {
                            var variaveis = JsonConvert.DeserializeObject<Dictionary<string, string>>(estadoAtual.Variaveis ?? "{}")
                                           ?? new Dictionary<string, string>();
                            variaveis[variavelNome] = mensagem;
                            estadoAtual.Variaveis = JsonConvert.SerializeObject(variaveis);
                        }
                    }

                    // 8. Busca a proxima etapa baseada na resposta
                    var proximaEtapa = await _unitOfWork.Flow.ObterProximaEtapa(estadoAtual.EtapaAtualId.Value, mensagem);

                    if (proximaEtapa == null)
                    {
                        // Nenhuma etapa correspondeu -> finaliza ou reenvia a mesma etapa
                        if (etapaAtual.NomeEtapa == "Capturar Input")
                        {
                            // Se estava capturando input, tenta avancar com "Qualquer_Resposta"
                            proximaEtapa = await _unitOfWork.Flow.ObterProximaEtapa(estadoAtual.EtapaAtualId.Value, "Qualquer_Resposta");
                        }

                        if (proximaEtapa == null)
                        {
                            estadoAtual.Finalizado = true;
                            estadoAtual.DataAtualizacao = DateTime.Now;
                            await _unitOfWork.ConversationState.Atualizar(estadoAtual);

                            _unitOfWork.Commit();
                            resultado.Sucesso = true;
                            resultado.FlowFinalizado = true;
                            resultado.Mensagem = "Flow finalizado.";
                            return resultado;
                        }
                    }

                    // 9. Executa a nova etapa
                    await ExecutarEtapa(proximaEtapa, estadoAtual.Variaveis, estadoAtual);

                    // 10. Atualiza o estado da conversa
                    estadoAtual.EtapaAtualId = proximaEtapa.Id;
                    estadoAtual.DataAtualizacao = DateTime.Now;
                    await _unitOfWork.ConversationState.Atualizar(estadoAtual);

                    resultado.Sucesso = true;
                    resultado.FlowId = estadoAtual.FlowId;
                    resultado.EtapaId = proximaEtapa.Id;
                }

                _unitOfWork.Commit();
            }
            catch (Exception)
            {
                _unitOfWork.Rollback();
                throw;
            }

            return resultado;
        }

        private async Task ExecutarEtapa(FlowEtapa etapa, string? variaveisJson, ConversationState? estadoAtual)
        {
            if (etapa.NomeEtapa == "Mensagem" && !string.IsNullOrEmpty(etapa.ConteudoLivre))
            {
                // Substitui variaveis no formato {{nome}} pelos valores capturados
                var mensagem = etapa.ConteudoLivre;

                if (!string.IsNullOrEmpty(variaveisJson))
                {
                    var variaveis = JsonConvert.DeserializeObject<Dictionary<string, string>>(variaveisJson);
                    if (variaveis != null)
                    {
                        foreach (var kvp in variaveis)
                        {
                            mensagem = mensagem.Replace($"{{{{{kvp.Key}}}}}", kvp.Value);
                        }
                    }
                }

                // Nota: O numero do contato precisa ser resolvido externamente.
                // O FlowOrchestrator e chamado apos a resolucao do Contato,
                // entao aqui apenas marcamos que uma mensagem deveria ser enviada.
                // O envio efetivo e feito pelo WebhookController apos obter o telefone.
                estadoAtual?.Variaveis?.ToString();
            }
        }

        private static string? ObterVariavelSaida(FlowEtapa etapa)
        {
            if (string.IsNullOrEmpty(etapa.ConteudoLivre))
                return null;

            // Tenta extrair o nome da variavel do conteudo livre
            // Formato esperado: "Digite seu nome" ou similar
            var match = Regex.Match(etapa.ConteudoLivre, @"\{\{(\w+)\}\}");
            if (match.Success)
                return match.Groups[1].Value;

            return null;
        }
    }
}
