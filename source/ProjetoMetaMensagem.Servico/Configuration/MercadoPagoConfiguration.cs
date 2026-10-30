namespace ProjetoMetaMensagem.Servico.Configuration
{
    // Conta Mercado Pago da Contact Solution (secao MercadoPago; no Render, MercadoPago__AccessToken
    // etc.). As cobrancas aos clientes viram orders Pix nela; o valor inteiro fica na conta e o
    // acerto com cada empresa e feito por fora (o Mercado Pago nao tem o split do Asaas).
    public class MercadoPagoConfiguration
    {
        // So informativo (log de boot e diagnostico): o Mercado Pago usa o mesmo endereco para
        // teste e producao, quem decide e a credencial.
        public string Ambiente { get; set; } = string.Empty;

        public string UrlApi { get; set; } = "https://api.mercadopago.com";

        // Credencial usada nas chamadas (APP_USR-...). PublicKey, ClientId, ClientSecret e UserId
        // ficam guardados para o checkout/OAuth, sem uso na API de orders.
        public string? AccessToken { get; set; }
        public string? PublicKey { get; set; }
        public string? ClientId { get; set; }
        public string? ClientSecret { get; set; }
        public string? UserId { get; set; }

        // "Assinatura secreta" do painel (Suas integracoes > Webhooks), diferente do AccessToken.
        // Sem ela o webhook aceita qualquer chamada -- a baixa ainda reconsulta a order.
        public string? WebhookSecret { get; set; }

        // A order exige e-mail do pagador e muitos contatos nao tem. Este vai no lugar: o Pix pode
        // ser pago por qualquer pessoa e a baixa casa pelo external_reference, nao pelo pagador.
        public string? EmailPagadorPadrao { get; set; }

        public bool Configurada => !string.IsNullOrWhiteSpace(AccessToken);
    }

    // Secao Cobranca: qual provedor gera os Pix novos (ver ProvedorCobranca). As cobrancas ja
    // geradas continuam no provedor delas.
    public class CobrancaConfiguration
    {
        public string Provedor { get; set; } = Dominio.Interfaces.Servicos.ProvedorCobranca.MercadoPago;
    }
}
