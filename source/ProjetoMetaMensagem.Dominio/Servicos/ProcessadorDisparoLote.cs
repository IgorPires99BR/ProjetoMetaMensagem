using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using ProjetoMetaMensagem.Dominio.Entidades;
using ProjetoMetaMensagem.Dominio.Help.Error;
using ProjetoMetaMensagem.Dominio.Helpers;
using ProjetoMetaMensagem.Dominio.Helpers.MensagemFormatter;
using ProjetoMetaMensagem.Dominio.Interfaces;
using ProjetoMetaMensagem.Dominio.Interfaces.Servicos;
using ProjetoMetaMensagem.Dominio.Interfaces.Servicos.Meta;
using ProjetoMetaMensagem.Dominio.UseCases.Messages.EnviarMensagemTemplateMetaLote;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ProjetoMetaMensagem.Dominio.Servicos
{
    public enum ResultadoProcessamentoDisparo
    {
        SemItem,
        Processado,
        // Limite de envio, provedor fora ou erro inesperado: quem chama da um respiro.
        Instavel
    }

    public interface IProcessadorDisparoLote
    {
        Task<ResultadoProcessamentoDisparo> ProcessarProximoAsync();

        // Devolve a fila quem caiu antes de enviar e registra como INCERTO quem caiu enviando.
        Task ResolverInterrompidosAsync();
    }

    // Um contato por vez, com a situacao gravada a cada passo. A regra que rege tudo: depois de
    // marcar ENVIANDO, o item nunca volta sozinho para a fila, porque a mensagem pode ter saido.
    public class ProcessadorDisparoLote : IProcessadorDisparoLote
    {
        public const int MaximoDeTentativas = 5;

        // Maior que o pior caso de cada fase: Pix (ate 4 chamadas de 30s) e Meta (2 numeros, 100s cada).
        private const int SegundosDeReserva = 300;

        private const string MotivoIncerto = "A Meta não confirmou o envio. Confira no WhatsApp se a mensagem chegou.";
        private const string OrientacaoIncerto = "A Meta não confirmou se esta mensagem saiu. Confira no WhatsApp: se não chegou, inative a cobrança (quando houver) e envie de novo.";

        private readonly IUnitOfWork _unitOfWork;
        private readonly IMetaService _meta;
        private readonly IPreparadorCobrancaPix _preparadorPix;
        private readonly ILogger<ProcessadorDisparoLote> _logger;

        public ProcessadorDisparoLote(IUnitOfWork unitOfWork, IMetaService meta, IPreparadorCobrancaPix preparadorPix,
                                      ILogger<ProcessadorDisparoLote> logger)
        {
            _unitOfWork = unitOfWork;
            _meta = meta;
            _preparadorPix = preparadorPix;
            _logger = logger;
        }

        public async Task<ResultadoProcessamentoDisparo> ProcessarProximoAsync()
        {
            var item = await _unitOfWork.DisparoLote.ReservarProximo(SegundosDeReserva);
            if (item == null) return ResultadoProcessamentoDisparo.SemItem;

            try
            {
                return await ProcessarAsync(item);
            }
            catch (Exception ex)
            {
                // A situacao gravada decide o resto: PREPARANDO volta para a fila quando a reserva
                // vencer e ENVIANDO vira INCERTO (ver ResolverInterrompidosAsync).
                _logger.LogError(ex, "Disparo em lote: erro ao processar o item {ItemId} do lote {LoteId}.", item.Id, item.LoteId);
                return ResultadoProcessamentoDisparo.Instavel;
            }
        }

        public async Task ResolverInterrompidosAsync()
        {
            await _unitOfWork.DisparoLote.LiberarPreparacoesInterrompidas(MaximoDeTentativas);

            var item = await _unitOfWork.DisparoLote.ReservarEnvioInterrompido(SegundosDeReserva);
            if (item == null) return;

            _logger.LogWarning("Disparo em lote: item {ItemId} do lote {LoteId} interrompido durante o envio; registrado como incerto.", item.Id, item.LoteId);

            var lote = await _unitOfWork.DisparoLote.ObterPorId(item.LoteId);
            var envio = item.EnvioJson != null ? JsonConvert.DeserializeObject<EnviarMensagemTemplateMetaLoteCommand>(item.EnvioJson) : null;
            if (lote == null || envio == null)
            {
                await _unitOfWork.DisparoLote.Finalizar(item.Id, StatusDisparoItem.Incerto, OrientacaoIncerto);
                return;
            }

            var template = await _unitOfWork.Template.ObterPorIdEEmpresa(lote.TemplateId, lote.EmpresaId);
            var resultados = envio.Telefones.Select(telefone => new ResultadoEnvioTemplate
            {
                Sucesso = false,
                TipoFalha = TipoFalhaEnvio.Incerta,
                ContatoId = item.ContatoId.ToString(),
                Telefone = telefone
            }).ToList();

            await RegistrarAsync(item, lote, template, envio, CobrancaDo(item), resultados, StatusDisparoItem.Incerto, OrientacaoIncerto);
        }

        private async Task<ResultadoProcessamentoDisparo> ProcessarAsync(DisparoLoteItem item)
        {
            var lote = await _unitOfWork.DisparoLote.ObterPorId(item.LoteId);
            if (lote == null)
            {
                await _unitOfWork.DisparoLote.Finalizar(item.Id, StatusDisparoItem.Falhou, "O lote deste envio não existe mais.");
                return ResultadoProcessamentoDisparo.Processado;
            }

            var template = await _unitOfWork.Template.ObterPorIdEEmpresa(lote.TemplateId, lote.EmpresaId);

            // Tentativa anterior ja preparou: reaproveita o mesmo Pix em vez de gerar outro.
            var envio = item.EnvioJson != null ? JsonConvert.DeserializeObject<EnviarMensagemTemplateMetaLoteCommand>(item.EnvioJson) : null;
            var cobranca = CobrancaDo(item);

            if (envio == null)
            {
                var preparo = await PrepararAsync(item, lote, template);
                if (preparo.Finalizado != null) return preparo.Finalizado.Value;

                envio = preparo.Envio!;
                cobranca = preparo.Cobranca;
                await _unitOfWork.DisparoLote.SalvarPreparacao(item.Id, JsonConvert.SerializeObject(envio),
                    cobranca != null ? JsonConvert.SerializeObject(cobranca) : null);
            }

            var phoneNumberId = await _unitOfWork.Empresa.ObterPhoneNumberId(lote.EmpresaId);
            var token = await _unitOfWork.Empresa.ObterMetaAccessToken(lote.EmpresaId);

            if (await _unitOfWork.DisparoLote.MarcarEnviando(item.Id, SegundosDeReserva) == 0)
                return ResultadoProcessamentoDisparo.Processado;

            var resultados = await _meta.EnviarTemplatesEmLoteAsync(envio, phoneNumberId, token);

            return await ConcluirAsync(item, lote, template, envio, cobranca, resultados);
        }

        private record Preparo(EnviarMensagemTemplateMetaLoteCommand? Envio, CobrancaCliente? Cobranca, ResultadoProcessamentoDisparo? Finalizado);

        private async Task<Preparo> PrepararAsync(DisparoLoteItem item, DisparoLote lote, Template? template)
        {
            async Task<Preparo> Falhou(string erro)
            {
                await _unitOfWork.DisparoLote.Finalizar(item.Id, StatusDisparoItem.Falhou, erro);
                return new Preparo(null, null, ResultadoProcessamentoDisparo.Processado);
            }

            // O modelo pode ter sido inativado depois de o lote entrar na fila.
            if (template == null) return await Falhou("O modelo desta mensagem não existe mais.");
            if (!template.Ativo) return await Falhou("O modelo foi inativado antes de este envio sair.");

            var contato = (await _unitOfWork.Contato.ObterPorIds(lote.EmpresaId, new[] { item.ContatoId })).FirstOrDefault();
            if (contato == null) return await Falhou("O contato não existe mais.");
            if (!contato.Ativo) return await Falhou("O contato foi inativado antes de este envio sair.");
            if (string.IsNullOrWhiteSpace(contato.Telefone)) return await Falhou("O contato está sem telefone cadastrado.");

            var contatoId = contato.Id.ToString();
            var envio = new EnviarMensagemTemplateMetaLoteCommand
            {
                IdEmpresa = lote.EmpresaId,
                EmpresaId = lote.EmpresaId,
                TemplateId = template.Id,
                NomeTemplate = template.NomeTemplate,
                Idioma = template.Idioma ?? "pt_BR",
                ParametroHeaderMediaUrl = lote.ParametroHeaderMediaUrl,
                ParametrosButton = Desserializar<List<string>>(lote.ParametrosButtonJson) ?? new List<string>(),
                Telefones = new List<string> { contato.Telefone },
                ContatosIds = new List<string> { contatoId },
                Origem = Common.OrigemDisparo.DisparadorDeMensagem
            };

            // Telefone2 (dois socios, por exemplo) recebe a mesma mensagem, com a mesma cobranca.
            if (!string.IsNullOrWhiteSpace(contato.Telefone2)
                && TelefoneHelper.FormatarParaMeta(contato.Telefone2) != TelefoneHelper.FormatarParaMeta(contato.Telefone))
            {
                envio.Telefones.Add(contato.Telefone2);
                envio.ContatosIds.Add(contatoId);
            }

            var agora = HorarioBrasilia.Agora();
            var variaveis = Desserializar<List<AgendamentoVariavelDto>>(lote.VariaveisJson) ?? new List<AgendamentoVariavelDto>();
            if (variaveis.Count > 0)
            {
                var parametros = (await _unitOfWork.Parametro.ObterPorEmpresa(lote.EmpresaId)).ToDictionary(p => p.Id);
                var destinatarios = envio.Telefones.Select(telefone => (telefone, contato));
                ResolvedorDeVariaveis.Preencher(envio, variaveis, destinatarios, parametros, agora);
            }

            // Antes do Pix: dado faltando no contato nao pode gerar cobranca que nao vai sair.
            var validacao = new EnviarMensagemTemplateMetaLoteValidator().Validate(envio);
            if (!validacao.IsValid) return await Falhou(validacao.Errors[0].ErrorMessage);

            CobrancaPreparada? preparada = null;
            Dictionary<Guid, CobrancaPreparada>? preparadas = null;
            if (template.GeraCobranca)
            {
                preparadas = await _preparadorPix.PrepararAsync(lote.EmpresaId, template, new[] { contato }, agora);
                preparada = preparadas[contato.Id];
            }

            var falhas = _preparadorPix.AplicarNoLote(envio, template, preparadas);
            if (envio.Telefones.Count == 0)
            {
                var erro = falhas.Select(f => f.Erro).FirstOrDefault() ?? "Não foi possível preparar a mensagem.";

                if (preparada is { ErroTransitorio: true } && item.Tentativas < MaximoDeTentativas)
                {
                    await _unitOfWork.DisparoLote.AgendarNovaTentativa(item.Id, EsperaDaTentativa(item.Tentativas), erro);
                    return new Preparo(null, null, ResultadoProcessamentoDisparo.Instavel);
                }

                if (preparada != null) await _preparadorPix.DescartarAsync(lote.EmpresaId, new[] { preparada });
                return await Falhou(erro);
            }

            return new Preparo(envio, preparada?.Cobranca, null);
        }

        private async Task<ResultadoProcessamentoDisparo> ConcluirAsync(DisparoLoteItem item, DisparoLote lote, Template? template,
            EnviarMensagemTemplateMetaLoteCommand envio, CobrancaCliente? cobranca, List<ResultadoEnvioTemplate> resultados)
        {
            var aceitos = resultados.Count(r => r.Sucesso);
            var incertos = resultados.Count(r => !r.Sucesso && r.TipoFalha == TipoFalhaEnvio.Incerta);

            if (aceitos > 0)
            {
                // Um numero recebeu: o contato foi cobrado. Repetir mandaria de novo para quem ja tem.
                var ressalva = aceitos < resultados.Count
                    ? $"Enviado, mas {resultados.Count - aceitos} número(s) do contato não receberam: {ErroLegivel(resultados.First(r => !r.Sucesso).Erro)}"
                    : null;
                await RegistrarAsync(item, lote, template, envio, cobranca, resultados, StatusDisparoItem.Enviado, ressalva);
                return ResultadoProcessamentoDisparo.Processado;
            }

            if (incertos > 0)
            {
                await RegistrarAsync(item, lote, template, envio, cobranca, resultados, StatusDisparoItem.Incerto, OrientacaoIncerto);
                return ResultadoProcessamentoDisparo.Processado;
            }

            var erro = ErroLegivel(resultados.Select(r => r.Erro).FirstOrDefault());

            if (resultados.All(r => r.TipoFalha == TipoFalhaEnvio.Transitoria) && item.Tentativas < MaximoDeTentativas)
            {
                await _unitOfWork.DisparoLote.AgendarNovaTentativa(item.Id, EsperaDaTentativa(item.Tentativas), erro);
                return ResultadoProcessamentoDisparo.Instavel;
            }

            // Ninguem recebeu este Pix: nao pode ficar pagavel no provedor.
            if (cobranca != null)
                await _preparadorPix.DescartarAsync(lote.EmpresaId, new[] { new CobrancaPreparada { Cobranca = cobranca } });

            await _unitOfWork.DisparoLote.Finalizar(item.Id, StatusDisparoItem.Falhou, erro);
            return ResultadoProcessamentoDisparo.Processado;
        }

        // Tudo junto: mensagem aceita sem cobranca registrada foi o que aconteceu no primeiro disparo
        // em massa. Se isto falhar, o item segue ENVIANDO e a recuperacao registra como incerto.
        private async Task RegistrarAsync(DisparoLoteItem item, DisparoLote lote, Template? template,
            EnviarMensagemTemplateMetaLoteCommand envio, CobrancaCliente? cobranca, List<ResultadoEnvioTemplate> resultados,
            string status, string? erro)
        {
            _unitOfWork.BeginTransaction();
            try
            {
                var cobrancaGravada = false;
                // A cobranca aponta para o envio confirmado, quando houver um.
                foreach (var (resultado, indice) in resultados.Select((r, i) => (r, i)).OrderByDescending(x => x.r.Sucesso))
                {
                    if (!resultado.Sucesso && resultado.TipoFalha != TipoFalhaEnvio.Incerta) continue;

                    var historico = new HistoricoDisparo
                    {
                        EmpresaId = lote.EmpresaId,
                        ContatoId = item.ContatoId,
                        TemplateId = lote.TemplateId,
                        TipoDisparo = "Template",
                        WamidMeta = resultado.Sucesso ? resultado.WamidMeta : null,
                        Conteudo = TemplateTextoHelper.MontarTextoEnviado(template?.Conteudo, envio.NomeTemplate, envio.ParametrosBodyDoDestinatario(indice)),
                        PayloadEnvio = resultado.JsonEnviado,
                        Origem = envio.Origem,
                        MotivoFalha = resultado.Sucesso ? null : MotivoIncerto
                    };
                    await _unitOfWork.HistoricoDisparo.Incluir(historico);

                    if (cobranca != null && !cobrancaGravada)
                    {
                        cobranca.HistoricoDisparoId = historico.Id;
                        await _unitOfWork.CobrancaCliente.Incluir(cobranca);
                        cobrancaGravada = true;
                    }
                }

                await _unitOfWork.DisparoLote.Finalizar(item.Id, status, erro);
                _unitOfWork.Commit();
            }
            catch
            {
                _unitOfWork.Rollback();
                throw;
            }
        }

        private static CobrancaCliente? CobrancaDo(DisparoLoteItem item) => Desserializar<CobrancaCliente>(item.CobrancaJson);

        private static T? Desserializar<T>(string? json) where T : class =>
            string.IsNullOrWhiteSpace(json) ? null : JsonConvert.DeserializeObject<T>(json);

        private static int EsperaDaTentativa(int tentativas) => tentativas switch
        {
            1 => 30,
            2 => 60,
            3 => 120,
            _ => 300
        };

        // A Meta devolve o corpo cru em JSON: na tela vai so o texto.
        private static string ErroLegivel(string? erro)
        {
            if (string.IsNullOrWhiteSpace(erro)) return "A Meta não aceitou o envio.";
            return erro.TrimStart().StartsWith("{") ? MetaApiException.DoCorpo(erro, "o envio").Message : erro;
        }
    }
}
