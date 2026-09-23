using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using ProjetoMetaMensagem.Dominio.Common;
using ProjetoMetaMensagem.Dominio.Entidades;
using ProjetoMetaMensagem.Dominio.Help.Error;
using ProjetoMetaMensagem.Dominio.Interfaces;
using ProjetoMetaMensagem.Dominio.Interfaces.Mediator;
using ProjetoMetaMensagem.Dominio.Helpers.MensagemFormatter;
using ProjetoMetaMensagem.Dominio.Interfaces.Servicos;
using ProjetoMetaMensagem.Dominio.Servicos;
using ProjetoMetaMensagem.Dominio.UseCases.Numero.CriaNumero;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjetoMetaMensagem.Dominio.UseCases.Messages.EnviarMensagemTemplateMetaLote
{
    public class EnviarMensagemTemplateMetaLoteHandler : IRequestHandler<EnviarMensagemTemplateMetaLoteCommand, Response<EnviarMensagemTemplateMetaLoteResult>>
    {
        private readonly IMetaService _metaService;
        private readonly IUnitOfWork _unitOfWork;


        private readonly ILogger<EnviarMensagemTemplateMetaLoteHandler> _logger;

        public EnviarMensagemTemplateMetaLoteHandler(IMetaService metaService, IUnitOfWork unitOfWork, ILogger<EnviarMensagemTemplateMetaLoteHandler> logger)
        {
            _metaService = metaService;
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async Task<Response<EnviarMensagemTemplateMetaLoteResult>> Handle(EnviarMensagemTemplateMetaLoteCommand command)
        {

            var response = new Response<EnviarMensagemTemplateMetaLoteResult>();

            try
            {
                // Antes da validacao: os parametros do corpo que ela confere sao os que sairam
                // daqui. O Agendamento ja chega com tudo resolvido (Variaveis vazio).
                if (command.Variaveis != null && command.Variaveis.Count > 0)
                {
                    var erro = await ResolverVariaveisPorContato(command);
                    if (erro != null)
                    {
                        response.AddErro(erro);
                        return response;
                    }
                }
            }
            catch (Exception ex)
            {
                response.AddErroServico(ex, _logger, nameof(EnviarMensagemTemplateMetaLoteHandler));
                return response;
            }

            var validator = new EnviarMensagemTemplateMetaLoteValidator();
            var validateResult = validator.Validate(command);

            if (!validateResult.IsValid)
            {
                response.AddErros(validateResult.Errors.ToCustomValidationFailure());
                return response;
            }

            try
            {
                var phoneNumberId = await _unitOfWork.Empresa.ObterPhoneNumberId(command.IdEmpresa);
                var token = await _unitOfWork.Empresa.ObterMetaAccessToken(command.IdEmpresa);

                // O serviço retorna o Dictionary<string, ResultadoEnvioTemplate> contendo [Telefone -> Resultado]
                var resultadoDisparos = await _metaService.EnviarTemplatesEmLoteAsync(command, phoneNumberId, token);

                // Uma consulta so; o texto muda por destinatario quando o disparo e personalizado.
                var templateEnviado = command.TemplateId.HasValue
                    ? await _unitOfWork.Template.ObterPorIdEEmpresa(command.TemplateId.Value, command.IdEmpresa)
                    : null;

                // Contatos de quem GeraCobranca vai precisar, buscados de uma vez so (nao um a
                // um dentro do loop) -- so quando o template dispara cobranca, pra nao pagar
                // essa consulta em todo disparo em lote comum.
                Dictionary<Guid, Entidades.Contato>? contatoPorId = null;
                if (templateEnviado != null && templateEnviado.GeraCobranca)
                {
                    var idsSucesso = resultadoDisparos
                        .Where(r => r.Sucesso)
                        .Select(r => Guid.TryParse(r.ContatoId, out var id) ? id : (Guid?)null)
                        .Where(id => id.HasValue)
                        .Select(id => id!.Value)
                        .Distinct()
                        .ToList();

                    contatoPorId = (await _unitOfWork.Contato.ObterPorIds(command.IdEmpresa, idsSucesso))
                        .ToDictionary(c => c.Id);
                }

                var agora = DateTime.Now;

                // Um resultado por destinatario, na ordem da lista: o indice diz de quem e cada
                // um, mesmo quando dois contatos dividem o mesmo telefone.
                for (var i = 0; i < resultadoDisparos.Count; i++)
                {
                    var respostaMeta = resultadoDisparos[i];

                    if (respostaMeta.Sucesso)
                    {
                        var contatoId = Guid.Parse(respostaMeta.ContatoId);

                        var historico = new HistoricoDisparo
                        {
                            EmpresaId = command.IdEmpresa,
                            // ✅ Recupera o ID específico e correto que mapeamos para este número de telefone
                            ContatoId = contatoId,
                            TemplateId = command.TemplateId,
                            TipoDisparo = "Template",
                            WamidMeta = respostaMeta.WamidMeta,
                            // Texto legivel pro usuario, com os valores que ESTE contato
                            // recebeu; o JSON da Meta vai na coluna de auditoria, sem poluir
                            // o que aparece no chat e no relatorio.
                            Conteudo = TemplateTextoHelper.MontarTextoEnviado(
                                templateEnviado?.Conteudo,
                                command.NomeTemplate,
                                command.ParametrosBodyDoDestinatario(i)),
                            PayloadEnvio = respostaMeta.JsonEnviado,
                            Origem = command.Origem
                        };

                        await _unitOfWork.HistoricoDisparo.Incluir(historico);

                        // Mesma regra do envio individual (EnviarMensagemTemplateMetaHandler):
                        // template com GeraCobranca abre uma CobrancaCliente por destinatario.
                        if (contatoPorId != null && contatoPorId.TryGetValue(contatoId, out var contato))
                        {
                            var cobranca = CobrancaClienteFactory.Criar(contato, templateEnviado!.Id, historico.Id, agora);
                            await _unitOfWork.CobrancaCliente.Incluir(cobranca);
                        }
                        else if (templateEnviado != null && templateEnviado.GeraCobranca)
                        {
                            _logger.LogWarning(
                                "GeraCobranca: contato {ContatoId} nao encontrado ao abrir CobrancaCliente do disparo {HistoricoDisparoId}",
                                contatoId, historico.Id);
                        }
                    }
                }

                // 3. Montagem do objeto de resultado mantendo o dicionário original [Telefone -> bool] para a View do CRM
                // Os totais contam DESTINATARIOS; os relatorios continuam por telefone (contrato
                // com o front), entao um telefone repetido so vale sucesso se todos os envios
                // dele deram certo e junta os erros de quem falhou.
                var porTelefone = resultadoDisparos.GroupBy(r => r.Telefone ?? string.Empty).ToList();
                var resultadoLote = new EnviarMensagemTemplateMetaLoteResult
                {
                    RelatorioDisparos = porTelefone.ToDictionary(g => g.Key, g => g.All(r => r.Sucesso)),
                    RelatorioErros = porTelefone
                        .Where(g => g.Any(r => !r.Sucesso && !string.IsNullOrEmpty(r.Erro)))
                        .ToDictionary(g => g.Key, g => string.Join(" | ",
                            g.Where(r => !r.Sucesso && !string.IsNullOrEmpty(r.Erro)).Select(r => r.Erro).Distinct())),
                    TotalProcessado = resultadoDisparos.Count,
                    TotalSucesso = resultadoDisparos.Count(r => r.Sucesso),
                    TotalFalha = resultadoDisparos.Count(r => !r.Sucesso)
                };

                // Atribui o resultado de sucesso ao envelope da Response
                response.AddValue(resultadoLote);


            }
            catch (Exception ex) 
            {
                response.AddErroServico(ex, _logger, nameof(EnviarMensagemTemplateMetaLoteHandler));
            }


            return response;
        }

        // Devolve a mensagem de erro de negocio, ou null quando resolveu tudo. Os contatos vem
        // sempre do banco e recortados pela empresa do disparo -- o front so diz QUAIS, nunca o
        // que cada um tem (ex: valorFatura), senao daria pra forjar o valor cobrado.
        private async Task<string?> ResolverVariaveisPorContato(EnviarMensagemTemplateMetaLoteCommand command)
        {
            var variaveis = command.Variaveis;

            var origemInvalida = variaveis.FirstOrDefault(v => !ResolvedorDeVariaveis.OrigemValida(v.Origem));
            if (origemInvalida != null)
                return $"Origem de variável inválida: {origemInvalida.Origem}.";

            if (variaveis.Any(v => v.Origem == ResolvedorDeVariaveis.ParametroCadastrado && !v.ParametroId.HasValue))
                return "Selecione o parâmetro de cada variável que usa um parâmetro cadastrado.";

            var telefones = command.Telefones ?? new List<string>();
            var contatosIds = command.ContatosIds ?? new List<string>();
            if (contatosIds.Count == 0 || contatosIds.Count != telefones.Count)
                return "A lista de contatos selecionados está inconsistente. Refaça a seleção e tente novamente.";

            var ids = contatosIds.Select(id => Guid.TryParse(id, out var g) ? g : Guid.Empty).ToList();
            if (ids.Contains(Guid.Empty))
                return "Há um contato selecionado com identificador inválido. Refaça a seleção e tente novamente.";

            var contatos = new List<Entidades.Contato>();
            foreach (var lote in ids.Distinct().Chunk(1000))
            {
                contatos.AddRange(await _unitOfWork.Contato.ObterPorIds(command.IdEmpresa, lote));
            }
            var contatoPorId = contatos.ToDictionary(c => c.Id);

            if (ids.Any(id => !contatoPorId.ContainsKey(id)))
                return "Há contato selecionado que não pertence a esta empresa.";

            var parametros = (await _unitOfWork.Parametro.ObterPorEmpresa(command.IdEmpresa)).ToDictionary(p => p.Id);

            var usadosInexistentes = variaveis.Any(v => v.Origem == ResolvedorDeVariaveis.ParametroCadastrado
                && !parametros.ContainsKey(v.ParametroId!.Value));
            if (usadosInexistentes)
                return "Um dos parâmetros escolhidos não existe mais. Atualize a tela e selecione novamente.";

            var destinatarios = telefones.Select((telefone, i) => (telefone, contatoPorId[ids[i]]));
            ResolvedorDeVariaveis.Preencher(command, variaveis, destinatarios, parametros, DateTime.Now);

            return null;
        }
    }
}
