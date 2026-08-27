using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using ProjetoMetaMensagem.Dominio.Entidades;
using ProjetoMetaMensagem.Dominio.Helpers;
using ProjetoMetaMensagem.Dominio.Helpers.MensagemFormatter;
using ProjetoMetaMensagem.Dominio.Interfaces;
using ProjetoMetaMensagem.Dominio.Interfaces.Servicos;
using ProjetoMetaMensagem.Dominio.UseCases.Messages.EnviarMensagemTemplateMeta;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace ProjetoMetaMensagem.Servico.Cobranca
{
    // Fecha o ciclo do pós-venda usando o próprio produto: quem compra vira contato na conta de
    // operação da Contact Solution e recebe na hora uma mensagem no WhatsApp. Além de acelerar o
    // onboarding (o cliente responde ali mesmo e marca a call), é a melhor demonstração possível
    // de que a plataforma funciona -- ela mesma faz o atendimento de quem acabou de comprar.
    //
    // Configuração:
    //   CaktoConfiguration:EmpresaOperacaoId   -> a conta da Contact Solution que envia
    //   CaktoConfiguration:TemplateBoasVindas  -> nome do template aprovado na Meta
    //   CaktoConfiguration:IdiomaBoasVindas    -> idioma do template (padrão pt_BR)
    public class OnboardingComercialService : IOnboardingComercialService
    {
        // Segunda mensagem, nome fixo no código (não precisa de config nova/deploy no Render):
        // avisa que um especialista vai ligar para agendar a implementação. Enquanto a Meta não
        // aprovar o template, o envio falha silenciosamente (logado) sem afetar o resto do
        // onboarding -- passa a funcionar sozinho assim que for aprovado.
        private const string NomeTemplateAtendenteLiga = "pagamento_confirmado_atendente_liga";

        private readonly IUnitOfWork _unitOfWork;
        private readonly IMetaService _metaService;
        private readonly IConfiguration _configuration;
        private readonly ILogger<OnboardingComercialService> _logger;

        public OnboardingComercialService(
            IUnitOfWork unitOfWork,
            IMetaService metaService,
            IConfiguration configuration,
            ILogger<OnboardingComercialService> logger)
        {
            _unitOfWork = unitOfWork;
            _metaService = metaService;
            _configuration = configuration;
            _logger = logger;
        }

        public async Task ReceberNovoClienteAsync(string nomeComprador, string? telefoneComprador, string emailComprador, Guid empresaDoCliente, string? plano = null, bool avisarPagamentoConfirmado = true)
        {
            var empresaOperacaoId = LerEmpresaOperacao();

            if (empresaOperacaoId == Guid.Empty)
            {
                _logger.LogWarning("Onboarding: CaktoConfiguration:EmpresaOperacaoId nao configurado — comprador {Email} nao virou contato", emailComprador);
                return;
            }

            if (string.IsNullOrWhiteSpace(telefoneComprador))
            {
                _logger.LogWarning("Onboarding: comprador {Email} veio sem telefone — nada a fazer no WhatsApp", emailComprador);
                return;
            }

            var telefone = TelefoneHelper.FormatarParaMeta(telefoneComprador!);

            try
            {
                var contatoId = await GarantirContatoAsync(empresaOperacaoId, nomeComprador, telefone, emailComprador);
                await EnviarBoasVindasWhatsAppAsync(empresaOperacaoId, contatoId, nomeComprador, telefone);

                // O aviso de "pagamento confirmado, um atendente vai ligar" so faz sentido
                // quando houve pagamento. Numa conta cadastrada pela equipe antes de o cliente
                // pagar, essa mensagem seria mentira -- e mentira logo no primeiro contato.
                if (avisarPagamentoConfirmado)
                {
                    await EnviarAvisoAtendenteAsync(empresaOperacaoId, contatoId, nomeComprador, telefone, plano);
                }
            }
            catch (Exception ex)
            {
                // A venda já aconteceu e a conta já existe: isto aqui é acessório.
                _logger.LogError(ex, "Onboarding: falha ao receber o cliente {Email} no WhatsApp", emailComprador);
            }
        }

        private async Task<Guid> GarantirContatoAsync(Guid empresaOperacaoId, string nome, string telefone, string email)
        {
            var existente = await _unitOfWork.Contato.ObterPorTelefone(empresaOperacaoId, telefone);
            if (existente != null) return existente.Id;

            // Contato pertence a um usuário, não à empresa direto: usa o primeiro admin da conta
            // de operação, que é quem "recebe" o lead.
            var usuarios = await _unitOfWork.Usuario.ObterPorEmpresa(empresaOperacaoId);
            var dono = usuarios.FirstOrDefault(u => u.IsAdmin == true) ?? usuarios.FirstOrDefault();

            if (dono == null)
            {
                _logger.LogWarning("Onboarding: conta de operacao {EmpresaId} nao tem usuario — contato nao criado", empresaOperacaoId);
                return Guid.Empty;
            }

            var contato = new Contato
            {
                Id = Guid.NewGuid(),
                UsuarioId = dono.Id,
                Nome = nome,
                Telefone = telefone,
                Email = email,
                DataCriacao = DateTime.Now
            };

            await _unitOfWork.Contato.Incluir(contato);
            _logger.LogInformation("Onboarding: comprador {Nome} virou contato na conta de operacao", nome);

            return contato.Id;
        }

        private async Task EnviarBoasVindasWhatsAppAsync(Guid empresaOperacaoId, Guid contatoId, string nome, string telefone)
        {
            var nomeTemplate = _configuration["CaktoConfiguration:TemplateBoasVindas"];

            if (string.IsNullOrWhiteSpace(nomeTemplate))
            {
                // Sem template configurado o contato já foi criado, então o comercial consegue
                // chamar manualmente pelo Chats -- só não sai a mensagem automática.
                _logger.LogInformation("Onboarding: TemplateBoasVindas nao configurado — contato criado, sem envio automatico");
                return;
            }

            var phoneNumberId = await _unitOfWork.Empresa.ObterPhoneNumberId(empresaOperacaoId);
            var token = await _unitOfWork.Empresa.ObterMetaAccessToken(empresaOperacaoId);

            if (string.IsNullOrWhiteSpace(phoneNumberId) || string.IsNullOrWhiteSpace(token))
            {
                _logger.LogWarning("Onboarding: conta de operacao sem numero conectado — mensagem de boas-vindas nao enviada");
                return;
            }

            // O template e buscado pelo nome configurado: e dele que sai o Id (pro historico) e
            // o texto (pra gravar a mensagem legivel, em vez do payload).
            var templates = await _unitOfWork.Template.ObterPorEmpresa(empresaOperacaoId);
            var templateBoasVindas = templates.FirstOrDefault(t =>
                string.Equals(t.NomeTemplate, nomeTemplate, StringComparison.OrdinalIgnoreCase));

            var command = new EnviarMensagemTemplateMetaCommand
            {
                IdEmpresa = empresaOperacaoId,
                EmpresaId = empresaOperacaoId,
                ContatoId = contatoId,
                TemplateId = templateBoasVindas?.Id,
                Telefone = telefone,
                NomeTemplate = nomeTemplate!,
                Idioma = _configuration["CaktoConfiguration:IdiomaBoasVindas"] ?? "pt_BR",
                // O template de boas-vindas usa uma variável só: o primeiro nome de quem comprou.
                ParametrosBody = new System.Collections.Generic.List<string> { PrimeiroNome(nome) }
            };

            var resultado = await _metaService.EnviarTemplateAsync(command, phoneNumberId!, token!);

            if (resultado?.Sucesso == true)
            {
                // Sem gravar o historico, a mensagem chegava no cliente mas nao aparecia no
                // Chats: quem fosse atender via a resposta dele sem saber o que a plataforma
                // tinha mandado, e o relatorio nao contava esse envio.
                await _unitOfWork.HistoricoDisparo.Incluir(new HistoricoDisparo
                {
                    Id = Guid.NewGuid(),
                    EmpresaId = empresaOperacaoId,
                    ContatoId = contatoId,
                    TemplateId = command.TemplateId,
                    TipoDisparo = "Onboarding",
                    WamidMeta = resultado.WamidMeta,
                    Conteudo = TemplateTextoHelper.MontarTextoEnviado(templateBoasVindas?.Conteudo, nomeTemplate, command.ParametrosBody),
                    DataEnvio = DateTime.Now
                });

                _logger.LogInformation("Onboarding: boas-vindas enviadas no WhatsApp para {Telefone}", telefone);
            }
            else
            {
                _logger.LogWarning("Onboarding: Meta recusou as boas-vindas para {Telefone}: {Erro}", telefone, resultado?.Erro);
            }
        }

        // Roda com o mesmo template todo santo dia, mas so entrega de fato depois que a Meta
        // aprovar "pagamento_confirmado_atendente_liga" -- ate la, EnviarTemplateAsync devolve
        // Sucesso=false e o warning abaixo e o unico efeito.
        private async Task EnviarAvisoAtendenteAsync(Guid empresaOperacaoId, Guid contatoId, string nome, string telefone, string? plano)
        {
            if (contatoId == Guid.Empty) return;

            var phoneNumberId = await _unitOfWork.Empresa.ObterPhoneNumberId(empresaOperacaoId);
            var token = await _unitOfWork.Empresa.ObterMetaAccessToken(empresaOperacaoId);

            if (string.IsNullOrWhiteSpace(phoneNumberId) || string.IsNullOrWhiteSpace(token))
                return;

            var templates = await _unitOfWork.Template.ObterPorEmpresa(empresaOperacaoId);
            var templateAviso = templates.FirstOrDefault(t =>
                string.Equals(t.NomeTemplate, NomeTemplateAtendenteLiga, StringComparison.OrdinalIgnoreCase));

            var command = new EnviarMensagemTemplateMetaCommand
            {
                IdEmpresa = empresaOperacaoId,
                EmpresaId = empresaOperacaoId,
                ContatoId = contatoId,
                TemplateId = templateAviso?.Id,
                Telefone = telefone,
                NomeTemplate = NomeTemplateAtendenteLiga,
                Idioma = "pt_BR",
                ParametrosBody = new System.Collections.Generic.List<string> { PrimeiroNome(nome), FormatarPlano(plano) }
            };

            var resultado = await _metaService.EnviarTemplateAsync(command, phoneNumberId!, token!);

            if (resultado?.Sucesso == true)
            {
                await _unitOfWork.HistoricoDisparo.Incluir(new HistoricoDisparo
                {
                    Id = Guid.NewGuid(),
                    EmpresaId = empresaOperacaoId,
                    ContatoId = contatoId,
                    TemplateId = command.TemplateId,
                    TipoDisparo = "Onboarding",
                    WamidMeta = resultado.WamidMeta,
                    Conteudo = TemplateTextoHelper.MontarTextoEnviado(templateAviso?.Conteudo, NomeTemplateAtendenteLiga, command.ParametrosBody),
                    DataEnvio = DateTime.Now
                });

                _logger.LogInformation("Onboarding: aviso de atendente enviado no WhatsApp para {Telefone}", telefone);
            }
            else
            {
                _logger.LogWarning(
                    "Onboarding: template de aviso de atendente ainda nao entrega (provavelmente aguardando aprovacao da Meta) para {Telefone}: {Erro}",
                    telefone, resultado?.Erro);
            }
        }

        private static string FormatarPlano(string? plano)
        {
            if (string.IsNullOrWhiteSpace(plano)) return "assinado";
            return plano.Trim().ToUpperInvariant() switch
            {
                "STARTER" => "Starter",
                "PRO" => "Pro",
                _ => plano
            };
        }

        private Guid LerEmpresaOperacao()
        {
            var valor = _configuration["CaktoConfiguration:EmpresaOperacaoId"];
            return Guid.TryParse(valor, out var id) ? id : Guid.Empty;
        }

        // "João Pedro da Silva" -> "João": tratar pelo primeiro nome soa como gente, não cadastro.
        private static string PrimeiroNome(string nome)
        {
            var partes = (nome ?? string.Empty).Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries);
            return partes.Length > 0 ? partes[0] : nome;
        }
    }
}
