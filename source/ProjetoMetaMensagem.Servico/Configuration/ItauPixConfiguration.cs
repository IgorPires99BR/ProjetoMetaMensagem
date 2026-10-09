namespace ProjetoMetaMensagem.Servico.Configuration
{
    // Enderecos da API Pix do Itau. Defaults conforme o portal do desenvolvedor do Itau; ficam
    // configuraveis (ItauPix__UrlApiProducao etc. no Render) porque o Itau ja mudou caminho de
    // API entre versoes e nao queremos depender de deploy pra corrigir uma URL.
    public class ItauPixConfiguration
    {
        public string UrlTokenProducao { get; set; } = "https://sts.itau.com.br/api/oauth/token";
        public string UrlApiProducao { get; set; } = "https://secure.api.itau/pix_recebimentos/v2";
        public string UrlTokenSandbox { get; set; } = "https://devportal.itau.com.br/api/jwt";
        public string UrlApiSandbox { get; set; } = "https://devportal.itau.com.br/sandboxapi/pix_recebimentos_ext_v2/v2";

        // Endereco publico desta API: vai no botao do template (pagina do Pix), no cabecalho
        // (imagem do QR) e no webhook cadastrado no Itau.
        public string UrlPublicaApi { get; set; } = "https://contactsolution.onrender.com";

        // Consulta periodica das cobrancas pendentes. Rede de seguranca do webhook: o Itau
        // exige mTLS no nosso endpoint e o Render nao valida certificado de cliente, entao nao
        // da pra garantir que o webhook sempre chega.
        public int IntervaloConsultaMinutos { get; set; } = 10;
    }
}
