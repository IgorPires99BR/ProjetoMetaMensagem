using System;
using System.Threading.Tasks;

namespace ProjetoMetaMensagem.Dominio.Interfaces.Servicos
{
    public static class ProvedorCobranca
    {
        public const string Asaas = "ASAAS";
        public const string MercadoPago = "MERCADOPAGO";

        public static bool Valido(string? nome) => nome == Asaas || nome == MercadoPago;

        public static string NomeExibicao(string? nome) => nome switch
        {
            MercadoPago => "Mercado Pago",
            Asaas => "Asaas",
            _ => nome ?? string.Empty
        };
    }

    // Quem gera, consulta e cancela o Pix das cobrancas aos clientes. Cada CobrancaCliente guarda
    // o provedor que gerou o Pix dela: trocar o ativo (Cobranca:Provedor) nao pode deixar as
    // cobrancas ja enviadas sem baixa. Erros vem como PixException, com mensagem para o usuario.
    //
    // tokenConta: access token da conta Mercado Pago DA EMPRESA (cobranca em nome dela). Nulo =
    // conta da plataforma. O Asaas ignora: la a divisao e pelo split da propria cobranca.
    public interface IProvedorCobranca
    {
        string Nome { get; }

        // O Asaas so cria cobranca para customer com CPF/CNPJ; sem o do contato, usa um cliente
        // padrao da empresa. O Mercado Pago so pede e-mail.
        bool ExigeCpfCnpjDoPagador { get; }

        Task<PixGerado> CriarCobrancaAsync(NovaCobrancaPix cobranca);

        // Fonte de verdade da baixa: nada e marcado como pago sem passar aqui.
        Task<ConsultaCobrancaPix> ConsultarCobrancaAsync(string idExterno, string? tokenConta);

        Task CancelarCobrancaAsync(string idExterno, string? tokenConta);

        // Chamada leve: confirma a credencial da plataforma sem gerar cobranca.
        Task TestarCredenciaisAsync();
    }

    public interface IProvedoresCobranca
    {
        // O que gera os Pix das cobrancas da propria Contact Solution (empresa sem conta propria).
        IProvedorCobranca Ativo { get; }

        // Provedor de uma cobranca ja gerada, ou do destino da empresa. Nulo = Asaas: as cobrancas
        // anteriores a BD/58 nao gravavam o provedor e todas eram do Asaas.
        IProvedorCobranca Do(string? nome);
    }

    // Conexao da conta Mercado Pago da empresa pelo OAuth do app da Contact Solution
    // (MercadoPago:ClientId/ClientSecret). A empresa autoriza no site do Mercado Pago, volta com um
    // code e ele vira o par de tokens. O access token vale 180 dias; o refresh token muda a cada
    // renovacao, entao os dois sao gravados de novo sempre.
    public interface IMercadoPagoOAuth
    {
        bool Configurado { get; }

        // user_id da conta da propria Contact Solution: ela nao pode ser "vendedora" de si mesma.
        string? UserIdDaPlataforma { get; }

        string UrlAutorizacao(string state);
        Task<TokensMercadoPago> TrocarCodigoAsync(string code);
        Task<TokensMercadoPago> RenovarAsync(string refreshToken);
    }

    public record TokensMercadoPago(string AccessToken, string RefreshToken, string UserId, DateTime ExpiraEm);
}
