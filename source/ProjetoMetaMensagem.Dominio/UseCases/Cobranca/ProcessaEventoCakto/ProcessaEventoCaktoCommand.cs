using Newtonsoft.Json;
using ProjetoMetaMensagem.Dominio.Common;
using ProjetoMetaMensagem.Dominio.Interfaces.Mediator;
using System.Collections.Generic;

namespace ProjetoMetaMensagem.Dominio.UseCases.Cobranca.ProcessaEventoCakto
{
    // Formato do webhook da Cakto: um POST JSON com três campos no topo -- secret, event e data.
    // A validação de origem é a comparação do `secret` (a Cakto não assina o corpo com HMAC,
    // diferente da Meta), por isso ele viaja aqui dentro e nunca pode ser logado.
    public class ProcessaEventoCaktoCommand : IRequest<Response<ProcessaEventoCaktoResult>>
    {
        [JsonProperty("secret")]
        public string? Secret { get; set; }

        [JsonProperty("event")]
        public string? Evento { get; set; }

        [JsonProperty("data")]
        public DadosEventoCakto? Dados { get; set; }

        // Corpo cru, guardado para auditoria. Preenchido pelo controller; o Secret é removido
        // antes de gravar.
        public string? PayloadOriginal { get; set; }
    }

    public class DadosEventoCakto
    {
        [JsonProperty("id")]
        public string? Id { get; set; }

        [JsonProperty("amount")]
        public decimal? Valor { get; set; }

        [JsonProperty("status")]
        public string? Status { get; set; }

        [JsonProperty("customer")]
        public CompradorCakto? Comprador { get; set; }

        [JsonProperty("product")]
        public ProdutoCakto? Produto { get; set; }

        [JsonProperty("offer")]
        public OfertaCakto? Oferta { get; set; }

        [JsonProperty("subscription")]
        public AssinaturaCakto? Assinatura { get; set; }

        // Código curto do pedido: é o que aparece pro cliente e o que a Cakto usa no suporte.
        [JsonProperty("refId")]
        public string? RefId { get; set; }

        [JsonProperty("paymentMethodName")]
        public string? MetodoPagamento { get; set; }

        // --- Origem da venda ---
        // Chegam do checkout e não voltam depois: é a única chance de saber de qual anúncio,
        // post ou campanha veio este cliente.
        [JsonProperty("utm_source")]
        public string? UtmSource { get; set; }

        [JsonProperty("utm_medium")]
        public string? UtmMedium { get; set; }

        [JsonProperty("utm_campaign")]
        public string? UtmCampaign { get; set; }

        [JsonProperty("utm_term")]
        public string? UtmTerm { get; set; }

        [JsonProperty("utm_content")]
        public string? UtmContent { get; set; }

        [JsonProperty("sck")]
        public string? Sck { get; set; }

        // Identificador do clique no anúncio do Facebook e do navegador do visitante: é com eles
        // que a Conversions API da Meta casa a venda com o anúncio que a gerou.
        [JsonProperty("fbc")]
        public string? Fbc { get; set; }

        [JsonProperty("fbp")]
        public string? Fbp { get; set; }
    }

    public class CompradorCakto
    {
        [JsonProperty("id")]
        public string? Id { get; set; }

        [JsonProperty("name")]
        public string? Nome { get; set; }

        [JsonProperty("email")]
        public string? Email { get; set; }

        [JsonProperty("phone")]
        public string? Telefone { get; set; }

        [JsonProperty("docNumber")]
        public string? Documento { get; set; }
    }

    public class ProdutoCakto
    {
        [JsonProperty("id")]
        public string? Id { get; set; }

        [JsonProperty("name")]
        public string? Nome { get; set; }
    }

    public class OfertaCakto
    {
        [JsonProperty("id")]
        public string? Id { get; set; }

        [JsonProperty("name")]
        public string? Nome { get; set; }
    }

    public class AssinaturaCakto
    {
        [JsonProperty("id")]
        public string? Id { get; set; }

        [JsonProperty("status")]
        public string? Status { get; set; }

        [JsonProperty("next_payment_date")]
        public string? ProximaCobranca { get; set; }
    }
}
