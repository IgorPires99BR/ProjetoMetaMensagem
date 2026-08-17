namespace ProjetoMetaMensagem.Servico.Configuration
{
    // Credenciais da Conversions API. O PixelId é o mesmo do pixel da landing; o token é
    // gerado no Gerenciador de Eventos da Meta e NÃO é o mesmo access token da WABA.
    public class MetaConversoesConfiguration
    {
        public string? PixelId { get; set; }
        public string? AccessToken { get; set; }

        // Código de teste do Gerenciador de Eventos: quando preenchido, o evento aparece na aba
        // "Testar eventos" e não conta como conversão real. Útil pra validar sem sujar o dado.
        public string? TestEventCode { get; set; }
    }
}
