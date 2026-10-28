namespace ProjetoMetaMensagem.Servico.Configuration
{
    // Conta Asaas da PLATAFORMA (Asaas__ApiKey etc. no Render). Uma conta so para todas as
    // empresas: o Pix cai nela e o split repassa a parte de cada empresa para a subconta dela.
    // URLs configuraveis para nao depender de deploy se o Asaas mudar algum endereco.
    public class AsaasConfiguration
    {
        public const string AmbienteSandbox = "SANDBOX";
        public const string AmbienteProducao = "PRODUCAO";

        public string Ambiente { get; set; } = AmbienteSandbox;

        public string UrlApiProducao { get; set; } = "https://api.asaas.com/v3";
        public string UrlApiSandbox { get; set; } = "https://api-sandbox.asaas.com/v3";

        // Chave da conta da plataforma ($aact_...). A conta precisa ser PJ (so PJ cria
        // subconta) e ter chave Pix cadastrada (sem ela o QR so vale ate 23:59 do dia).
        public string? ApiKey { get; set; }

        // Token escolhido por nos ao cadastrar o webhook no painel do Asaas (32 a 255 caracteres,
        // diferente da ApiKey). O Asaas devolve no cabecalho asaas-access-token.
        public string? WebhookToken { get; set; }

        // O Asaas exige User-Agent nas contas criadas a partir de 2024.
        public string UserAgent { get; set; } = "ContactSolution";

        // Taxa do Asaas por Pix recebido, em R$. O split e calculado sobre o valor LIQUIDO:
        // se a comissao nao cobrir a taxa, o repasse fixo passa do liquido e o Asaas bloqueia
        // o split (PAYMENT_SPLIT_DIVERGENCE_BLOCK). Conferida antes de gerar o Pix.
        public decimal TaxaPix { get; set; } = 1.99m;

        // Endereco publico desta API: vai no botao do template (pagina do Pix) e no cabecalho
        // (imagem do QR).
        public string UrlPublicaApi { get; set; } = "https://contactsolution.onrender.com";

        // Consulta periodica das cobrancas pendentes. Rede de seguranca do webhook.
        public int IntervaloConsultaMinutos { get; set; } = 10;

        public bool Configurada => !string.IsNullOrWhiteSpace(ApiKey);

        public bool Producao => string.Equals(Ambiente, AmbienteProducao, StringComparison.OrdinalIgnoreCase);

        // Chave de producao comeca com $aact_prod_ e a de sandbox com $aact_hmlg_. Chave do
        // ambiente errado so volta 401 sem motivo do Asaas; aqui o erro diz o que trocar.
        // Formato desconhecido (chave antiga) nao e barrado.
        public string? ErroChaveDoAmbiente()
        {
            var chave = (ApiKey ?? string.Empty).Trim().TrimStart('$');
            if (Producao && chave.StartsWith("aact_hmlg_"))
                return "A chave do Asaas configurada é de SANDBOX, mas Asaas:Ambiente está PRODUCAO.";
            if (!Producao && chave.StartsWith("aact_prod_"))
                return "A chave do Asaas configurada é de PRODUÇÃO, mas Asaas:Ambiente está SANDBOX.";
            return null;
        }
    }
}
