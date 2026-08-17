using Microsoft.Extensions.Logging;
using ProjetoMetaMensagem.Dominio.Common;
using ProjetoMetaMensagem.Dominio.Entidades;
using ProjetoMetaMensagem.Dominio.Help.Error;
using ProjetoMetaMensagem.Dominio.Interfaces;
using ProjetoMetaMensagem.Dominio.Interfaces.Mediator;
using ProjetoMetaMensagem.Dominio.Interfaces.Servicos;
using System;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;

namespace ProjetoMetaMensagem.Dominio.UseCases.Cobranca.ProcessaEventoCakto
{
    // Traduz um evento de pagamento da Cakto em efeito na plataforma: conta criada, liberada
    // ou suspensa. É a única porta de entrada de cliente novo -- a conta nasce do pagamento.
    public class ProcessaEventoCaktoHandler : IRequestHandler<ProcessaEventoCaktoCommand, Response<ProcessaEventoCaktoResult>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IEmailService _emailService;
        private readonly IConfiguracaoOfertasCakto _configuracaoOfertas;
        private readonly IOnboardingComercialService _onboarding;
        private readonly IConversoesMetaService _conversoesMeta;
        private readonly ILogger<ProcessaEventoCaktoHandler> _logger;

        public ProcessaEventoCaktoHandler(
            IUnitOfWork unitOfWork,
            IEmailService emailService,
            IConfiguracaoOfertasCakto configuracaoOfertas,
            IOnboardingComercialService onboarding,
            IConversoesMetaService conversoesMeta,
            ILogger<ProcessaEventoCaktoHandler> logger)
        {
            _unitOfWork = unitOfWork;
            _emailService = emailService;
            _configuracaoOfertas = configuracaoOfertas;
            _onboarding = onboarding;
            _conversoesMeta = conversoesMeta;
            _logger = logger;
        }

        public async Task<Response<ProcessaEventoCaktoResult>> Handle(ProcessaEventoCaktoCommand command)
        {
            var response = new Response<ProcessaEventoCaktoResult>();

            var validator = new ProcessaEventoCaktoValidator();
            var validateResult = validator.Validate(command);

            if (!validateResult.IsValid)
            {
                response.AddErros(validateResult.Errors.ToCustomValidationFailure());
                return response;
            }

            var evento = (command.Evento ?? string.Empty).Trim();
            var dados = command.Dados!;
            var eventoId = dados.Id!;

            try
            {
                _unitOfWork.BeginTransaction();

                // A Cakto reenvia o mesmo evento até 5 vezes se não receber 2xx em 8s. Sem esta
                // guarda, uma renovação reprocessada criaria conta e e-mail duplicados.
                if (await _unitOfWork.Assinatura.EventoJaProcessado(eventoId, evento))
                {
                    _unitOfWork.Commit();
                    response.AddValue(new ProcessaEventoCaktoResult { Acao = "evento-repetido-ignorado" });
                    return response;
                }

                var resultado = ClassificarEvento(evento) switch
                {
                    TipoEventoCakto.Libera => await LiberarAsync(command),
                    TipoEventoCakto.Suspende => await SuspenderAsync(command, cancelamento: true),
                    TipoEventoCakto.Inadimplente => await SuspenderAsync(command, cancelamento: false),
                    _ => new ProcessaEventoCaktoResult { Acao = $"evento-sem-efeito ({evento})" }
                };

                await _unitOfWork.Assinatura.RegistrarEvento(new EventoCakto
                {
                    EventoIdCakto = eventoId,
                    Evento = evento,
                    EmpresaId = resultado.EmpresaId,
                    PayloadJson = command.PayloadOriginal
                });

                _unitOfWork.Commit();

                _logger.LogInformation("Cakto: evento {Evento} ({EventoId}) -> {Acao}", evento, eventoId, resultado.Acao);
                response.AddValue(resultado);
            }
            catch (Exception ex)
            {
                _unitOfWork.Rollback();
                response.AddErroServico(ex, _logger, nameof(ProcessaEventoCaktoHandler));
            }

            return response;
        }

        // --- Efeitos ---

        private async Task<ProcessaEventoCaktoResult> LiberarAsync(ProcessaEventoCaktoCommand command)
        {
            var dados = command.Dados!;
            var email = dados.Comprador?.Email?.Trim();

            if (string.IsNullOrWhiteSpace(email))
            {
                // Sem e-mail não há como criar acesso nem reencontrar a conta depois.
                return new ProcessaEventoCaktoResult { Acao = "sem-email-do-comprador" };
            }

            var assinatura = await EncontrarAssinaturaAsync(dados);
            var contaCriada = false;
            Guid empresaId;

            if (assinatura != null)
            {
                empresaId = assinatura.EmpresaId;
            }
            else
            {
                // Cliente pode já existir (assinou de novo depois de cancelar, ou foi cadastrado
                // manualmente antes da integração): nesse caso reaproveita a empresa do usuário.
                var usuarioExistente = await _unitOfWork.Usuario.ObterPorEmail(email);

                if (usuarioExistente != null)
                {
                    empresaId = usuarioExistente.EmpresaId;
                }
                else
                {
                    empresaId = await CriarContaAsync(command, email);
                    contaCriada = true;
                }

                assinatura = await _unitOfWork.Assinatura.ObterPorEmpresa(empresaId);
            }

            var plano = MapearPlano(dados);
            var proximaCobranca = CalcularProximaCobranca(dados);

            if (assinatura == null)
            {
                assinatura = new Assinatura
                {
                    EmpresaId = empresaId,
                    AssinaturaIdCakto = dados.Assinatura?.Id,
                    ClienteIdCakto = dados.Comprador?.Id,
                    OfertaIdCakto = dados.Oferta?.Id,
                    EmailComprador = email,
                    Plano = plano,
                    Status = StatusAssinatura.Ativa,
                    ValorCentavos = ConverterCentavos(dados.Valor),
                    DataProximaCobranca = proximaCobranca,
                    EventoIdCakto = dados.Id,
                    UltimoEvento = command.Evento,
                    DataUltimoEvento = DateTime.Now,

                    // Origem so na criacao: a renovacao de daqui a tres meses nao carrega UTM
                    // nenhum, e sobrescrever apagaria de qual anuncio o cliente veio.
                    UtmSource = dados.UtmSource,
                    UtmMedium = dados.UtmMedium,
                    UtmCampaign = dados.UtmCampaign,
                    UtmTerm = dados.UtmTerm,
                    UtmContent = dados.UtmContent,
                    Sck = dados.Sck,
                    Fbc = dados.Fbc,
                    Fbp = dados.Fbp,
                    RefIdCakto = dados.RefId,
                    MetodoPagamento = dados.MetodoPagamento
                };

                await _unitOfWork.Assinatura.Incluir(assinatura);
            }
            else
            {
                assinatura.AssinaturaIdCakto = dados.Assinatura?.Id ?? assinatura.AssinaturaIdCakto;
                assinatura.ClienteIdCakto = dados.Comprador?.Id ?? assinatura.ClienteIdCakto;
                assinatura.OfertaIdCakto = dados.Oferta?.Id ?? assinatura.OfertaIdCakto;
                assinatura.EmailComprador = email;
                assinatura.Plano = plano;
                assinatura.Status = StatusAssinatura.Ativa;
                assinatura.ValorCentavos = ConverterCentavos(dados.Valor) ?? assinatura.ValorCentavos;
                assinatura.DataProximaCobranca = proximaCobranca ?? assinatura.DataProximaCobranca;
                assinatura.DataCancelamento = null;
                assinatura.EventoIdCakto = dados.Id;
                assinatura.UltimoEvento = command.Evento;
                assinatura.DataUltimoEvento = DateTime.Now;
                assinatura.MetodoPagamento = dados.MetodoPagamento ?? assinatura.MetodoPagamento;

                await _unitOfWork.Assinatura.Alterar(assinatura);
            }

            await AtualizarStatusEmpresaAsync(empresaId, "Ativo", plano);

            // Fecha o ciclo do anuncio: a Meta so sabia quem comecou conversa, nunca quem pagou.
            // Fora da transacao de proposito -- falha de rede na Meta nao pode desfazer a venda.
            await ReportarCompraAsync(assinatura, dados);

            return new ProcessaEventoCaktoResult
            {
                Acao = contaCriada ? "conta-criada-e-liberada" : "assinatura-liberada",
                EmpresaId = empresaId,
                Email = email,
                ContaCriada = contaCriada
            };
        }

        private async Task<ProcessaEventoCaktoResult> SuspenderAsync(ProcessaEventoCaktoCommand command, bool cancelamento)
        {
            var dados = command.Dados!;
            var assinatura = await EncontrarAssinaturaAsync(dados);

            if (assinatura == null)
            {
                return new ProcessaEventoCaktoResult { Acao = "assinatura-nao-encontrada", Email = dados.Comprador?.Email };
            }

            var reembolso = (command.Evento ?? string.Empty).ToLowerInvariant().Contains("refund");

            assinatura.Status = cancelamento
                ? (reembolso ? StatusAssinatura.Reembolsada : StatusAssinatura.Cancelada)
                : StatusAssinatura.Inadimplente;
            assinatura.DataCancelamento = cancelamento ? DateTime.Now : assinatura.DataCancelamento;
            assinatura.EventoIdCakto = dados.Id;
            assinatura.UltimoEvento = command.Evento;
            assinatura.DataUltimoEvento = DateTime.Now;

            await _unitOfWork.Assinatura.Alterar(assinatura);

            // A conta continua existindo e o cliente continua entrando pra ver os dados dele --
            // o que trava é o envio de mensagem, checado no disparo.
            await AtualizarStatusEmpresaAsync(assinatura.EmpresaId, cancelamento ? "Suspenso" : "Inadimplente", null);

            return new ProcessaEventoCaktoResult
            {
                Acao = cancelamento ? "assinatura-encerrada" : "assinatura-inadimplente",
                EmpresaId = assinatura.EmpresaId,
                Email = assinatura.EmailComprador
            };
        }

        private async Task<Guid> CriarContaAsync(ProcessaEventoCaktoCommand command, string email)
        {
            var dados = command.Dados!;
            var nomeComprador = string.IsNullOrWhiteSpace(dados.Comprador?.Nome) ? email : dados.Comprador!.Nome!;

            var empresa = new Entidades.Empresa
            {
                Id = Guid.NewGuid(),
                Nome = nomeComprador,
                Email = email,
                Cnpj = dados.Comprador?.Documento ?? string.Empty,
                Telefone = dados.Comprador?.Telefone,
                StatusConta = "Ativo",
                PlanoId = MapearPlano(dados),
                DataCriacao = DateTime.Now
            };

            var empresaId = await _unitOfWork.Empresa.Incluir(empresa);
            if (empresaId == Guid.Empty) empresaId = empresa.Id;

            var senhaProvisoria = GerarSenhaProvisoria();

            await _unitOfWork.Usuario.Incluir(new Entidades.Usuario
            {
                Id = Guid.NewGuid(),
                EmpresaId = empresaId,
                Nome = nomeComprador,
                Email = email,
                SenhaHash = BCrypt.Net.BCrypt.HashPassword(senhaProvisoria),
                IsAdmin = true,
                DataCriacao = DateTime.Now
            });

            // O e-mail sai fora da transação de propósito: falha de SMTP não pode desfazer uma
            // conta já paga. Se não chegar, o cliente usa "esqueci minha senha".
            await EnviarBoasVindasAsync(email, nomeComprador, senhaProvisoria);

            // Mesmo raciocínio: quem comprou vira contato na nossa própria conta e recebe o
            // WhatsApp de boas-vindas. O serviço engole as próprias falhas -- nada aqui pode
            // derrubar uma venda que já entrou.
            await _onboarding.ReceberNovoClienteAsync(nomeComprador, dados.Comprador?.Telefone, email, empresaId, empresa.PlanoId);

            return empresaId;
        }

        // Junta os identificadores que temos do comprador: o ctwa_clid (quando ele veio de um
        // anuncio Click-to-WhatsApp, gravado na primeira mensagem dele) ou fbc/fbp (quando veio
        // pela landing). Sem nenhum dos dois a Meta ainda aceita o evento pelo hash do e-mail,
        // com atribuicao mais fraca.
        private async Task ReportarCompraAsync(Assinatura assinatura, DadosEventoCakto dados)
        {
            try
            {
                var telefone = dados.Comprador?.Telefone;
                string? ctwaClid = null;

                if (!string.IsNullOrWhiteSpace(telefone))
                {
                    var origem = await _unitOfWork.OrigemLead.ObterPorTelefone(
                        assinatura.EmpresaId,
                        Helpers.TelefoneHelper.FormatarParaMeta(telefone!));

                    ctwaClid = origem?.CtwaClid;

                    if (origem != null && !origem.ConversaoEnviada)
                    {
                        await _unitOfWork.OrigemLead.MarcarConversaoEnviada(origem.Id);
                    }
                }

                await _conversoesMeta.ReportarCompraAsync(
                    assinatura.EmailComprador ?? string.Empty,
                    telefone,
                    (assinatura.ValorCentavos ?? 0) / 100m,
                    dados.RefId ?? dados.Id,
                    ctwaClid,
                    assinatura.Fbc,
                    assinatura.Fbp);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Falha ao reportar a venda de {Email} a Meta", assinatura.EmailComprador);
            }
        }

        private async Task AtualizarStatusEmpresaAsync(Guid empresaId, string status, string? plano)
        {
            var empresa = await _unitOfWork.Empresa.ObterPorId(empresaId);
            if (empresa == null) return;

            empresa.StatusConta = status;
            if (!string.IsNullOrWhiteSpace(plano)) empresa.PlanoId = plano;

            await _unitOfWork.Empresa.Alterar(empresa);
        }

        private async Task EnviarBoasVindasAsync(string email, string nomeCliente, string senhaProvisoria)
        {
            try
            {
                await _emailService.EnviarBoasVindasAsync(email, nomeCliente, senhaProvisoria);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Cakto: conta criada para {Email}, mas o e-mail de acesso falhou", email);
            }
        }

        // --- Apoio ---

        private async Task<Assinatura?> EncontrarAssinaturaAsync(DadosEventoCakto dados)
        {
            if (!string.IsNullOrWhiteSpace(dados.Assinatura?.Id))
            {
                var porId = await _unitOfWork.Assinatura.ObterPorAssinaturaCakto(dados.Assinatura!.Id!);
                if (porId != null) return porId;
            }

            var email = dados.Comprador?.Email?.Trim();
            if (string.IsNullOrWhiteSpace(email)) return null;

            return await _unitOfWork.Assinatura.ObterPorEmailComprador(email);
        }

        // Os nomes técnicos dos eventos variam por conta/versão do painel da Cakto, então a
        // classificação olha o significado no nome em vez de casar string exata -- assim um
        // "purchase_approved" e um "subscription_renewed" caem no mesmo lugar sem lista fixa.
        private static TipoEventoCakto ClassificarEvento(string evento)
        {
            var e = evento.ToLowerInvariant();

            if (e.Contains("refund") || e.Contains("chargeback") || e.Contains("cancel") || e.Contains("expired"))
                return TipoEventoCakto.Suspende;

            if (e.Contains("refus") || e.Contains("declin") || e.Contains("fail") || e.Contains("overdue") || e.Contains("paus"))
                return TipoEventoCakto.Inadimplente;

            if (e.Contains("approv") || e.Contains("paid") || e.Contains("renew") || e.Contains("created") || e.Contains("resum") || e.Contains("active"))
                return TipoEventoCakto.Libera;

            return TipoEventoCakto.SemEfeito;
        }

        // Descobre o plano pelo id da oferta (o jeito confiável, configurado em
        // CaktoConfiguration:Ofertas) e, sem configuração, cai no nome da oferta.
        //
        // A busca por nome casa PALAVRA INTEIRA: procurar "pro" solto dentro do texto acha
        // "produto", e um cliente do Starter viraria Pro -- exatamente o tipo de erro que só
        // aparece depois de alguém pagar. O nome do produto não entra na conta pelo mesmo
        // motivo: ele é igual para todos os planos.
        private string MapearPlano(DadosEventoCakto dados)
        {
            var idOferta = dados.Oferta?.Id;

            if (!string.IsNullOrWhiteSpace(idOferta))
            {
                var planoConfigurado = _configuracaoOfertas.PlanoDaOferta(idOferta!);
                if (!string.IsNullOrWhiteSpace(planoConfigurado)) return planoConfigurado!;
            }

            var nomeOferta = dados.Oferta?.Nome ?? string.Empty;

            if (ContemPalavra(nomeOferta, "enterprise") || ContemPalavra(nomeOferta, "corporativo"))
                return PlanoAssinatura.Enterprise;

            if (ContemPalavra(nomeOferta, "pro")) return PlanoAssinatura.Pro;

            return PlanoAssinatura.Starter;
        }

        private static bool ContemPalavra(string texto, string palavra) =>
            System.Text.RegularExpressions.Regex.IsMatch(
                texto ?? string.Empty,
                $@"\b{System.Text.RegularExpressions.Regex.Escape(palavra)}\b",
                System.Text.RegularExpressions.RegexOptions.IgnoreCase);

        private static int? ConverterCentavos(decimal? valor)
        {
            if (valor == null) return null;
            return (int)Math.Round(valor.Value * 100, MidpointRounding.AwayFromZero);
        }

        // A data informada pela Cakto manda; sem ela, calcula a partir do pagamento e do
        // intervalo da assinatura (ou de um mês, o ciclo dos nossos planos). Deixar em branco
        // faria o painel de Cobranças mostrar "—" onde deveria estar o vencimento, e ninguém
        // saberia quando esperar a renovação.
        private static DateTime? CalcularProximaCobranca(DadosEventoCakto dados)
        {
            var informada = ParseData(dados.Assinatura?.ProximaCobranca);
            if (informada != null) return informada;

            var pagamento = ParseData(dados.PagoEm) ?? DateTime.Now;
            var dias = dados.Assinatura?.DiasEntreCobrancas;

            return dias.HasValue && dias.Value > 0
                ? pagamento.AddDays(dias.Value)
                : pagamento.AddMonths(1);
        }

        private static DateTime? ParseData(string? valor)
        {
            if (string.IsNullOrWhiteSpace(valor)) return null;

            return DateTime.TryParse(valor, CultureInfo.InvariantCulture, DateTimeStyles.AdjustToUniversal, out var data)
                ? data.ToLocalTime()
                : null;
        }

        // Senha só serve para o primeiro acesso; o usuário troca depois.
        private static string GerarSenhaProvisoria()
        {
            const string alfabeto = "ABCDEFGHJKLMNPQRSTUVWXYZabcdefghijkmnopqrstuvwxyz23456789";
            var bytes = System.Security.Cryptography.RandomNumberGenerator.GetBytes(12);

            return new string(bytes.Select(b => alfabeto[b % alfabeto.Length]).ToArray());
        }

        private enum TipoEventoCakto
        {
            Libera,
            Suspende,
            Inadimplente,
            SemEfeito
        }
    }
}
