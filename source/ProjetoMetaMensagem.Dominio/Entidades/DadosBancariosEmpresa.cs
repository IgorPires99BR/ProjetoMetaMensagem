using System;

namespace ProjetoMetaMensagem.Dominio.Entidades
{
    // Onde a empresa recebe as cobrancas aos clientes dela (ver BD/51, 53, 54, 57 e 59). Ela escolhe
    // UMA conta (ProvedorRecebimento): Mercado Pago, conectada por OAuth, ou subconta Asaas. Com a
    // conta preenchida e a comissao definida, a cobranca sai em nome da empresa e a plataforma fica
    // so com a comissao; sem isso, a cobranca e da propria Contact Solution (ver DestinoRecebimento).
    //
    // As colunas da integracao direta com o Itau (agencia, conta, client id/secret,
    // certificado) e a chave Pix da Transfeera continuam na tabela, sem uso; ver BD/53 e BD/54.
    public class DadosBancariosEmpresa
    {
        public Guid EmpresaId { get; set; }

        // MERCADOPAGO (padrao) ou ASAAS (Interfaces.Servicos.ProvedorCobranca).
        public string ProvedorRecebimento { get; set; } = Interfaces.Servicos.ProvedorCobranca.MercadoPago;

        // Subconta criada pela plataforma (POST /v3/accounts). WalletId e o destino do split;
        // a apiKey (criptografada) so serve para consultar a subconta depois.
        public string? AsaasContaId { get; set; }
        public string? AsaasWalletId { get; set; }
        public string? AsaasApiKeyCriptografada { get; set; }
        public DateTime? AsaasContaCriadaEm { get; set; }

        // Conta Mercado Pago da empresa, autorizada por OAuth. A order e criada com o token DELA e a
        // comissao vai no marketplace_fee. Tokens criptografados; o access token vale 180 dias e o
        // job de renovacao troca antes de vencer (o refresh token muda a cada renovacao).
        public string? MercadoPagoUserId { get; set; }
        public string? MercadoPagoAccessTokenCriptografado { get; set; }
        public string? MercadoPagoRefreshTokenCriptografado { get; set; }
        public DateTime? MercadoPagoTokenExpiraEm { get; set; }
        public DateTime? MercadoPagoConectadoEm { get; set; }

        // Definida so pelo admin da plataforma. Nula = ainda nao combinada: a cobranca fica com a
        // Contact Solution, senao a plataforma repassaria 100% sem querer.
        public string? ComissaoTipo { get; set; }
        public decimal? ComissaoValor { get; set; }

        // Definido so pelo admin da plataforma (ver BD/57): forca a cobranca para a conta da
        // Contact Solution mesmo com a conta da empresa preenchida; o acerto e feito por fora.
        public bool RecebimentoNaPlataforma { get; set; }

        public bool CobrancaPixAtiva { get; set; }
        public DateTime DataCriacao { get; set; }
        public DateTime? DataAtualizacao { get; set; }

        public const string ComissaoPercentual = "PERCENTUAL";
        public const string ComissaoFixa = "FIXA";

        public bool ComissaoDefinida => ComissaoTipo != null && ComissaoValor.HasValue;
        public bool TemSubconta => !string.IsNullOrWhiteSpace(AsaasWalletId);
        public bool MercadoPagoConectado => !string.IsNullOrWhiteSpace(MercadoPagoAccessTokenCriptografado);

        // Conta Asaas propria da empresa, aberta fora da plataforma (a Sebrecon ja tinha uma e o
        // Asaas nao abre segunda conta no mesmo CNPJ). So o walletId e conhecido: sem AsaasContaId,
        // sem apiKey, e o GET /accounts da plataforma nao a enxerga.
        public bool CarteiraExterna => TemSubconta && string.IsNullOrWhiteSpace(AsaasContaId);

        public void DesconectarMercadoPago()
        {
            MercadoPagoUserId = null;
            MercadoPagoAccessTokenCriptografado = null;
            MercadoPagoRefreshTokenCriptografado = null;
            MercadoPagoTokenExpiraEm = null;
            MercadoPagoConectadoEm = null;
        }
    }
}
