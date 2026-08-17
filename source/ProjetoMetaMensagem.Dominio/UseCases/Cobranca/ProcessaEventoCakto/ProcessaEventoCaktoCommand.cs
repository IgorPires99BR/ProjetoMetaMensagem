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
