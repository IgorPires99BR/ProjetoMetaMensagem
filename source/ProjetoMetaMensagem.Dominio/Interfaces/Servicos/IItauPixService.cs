using ProjetoMetaMensagem.Dominio.Entidades;
using System;
using System.Threading.Tasks;

namespace ProjetoMetaMensagem.Dominio.Interfaces.Servicos
{
    // API Pix (padrao Bacen) do Itau, na conta da propria empresa: cada chamada usa as
    // credenciais e o certificado gravados em DadosBancariosEmpresa. Erros vem como
    // ItauPixException, com mensagem que pode ser mostrada ao usuario.
    public interface IItauPixService
    {
        // PUT /cob/{txid}: Pix imediato com validade em segundos a partir da criacao.
        Task<PixGerado> CriarCobrancaAsync(DadosBancariosEmpresa dados, string txid, decimal valor, int expiracaoSegundos, string? solicitacaoPagador);

        // GET /cob/{txid}. Fonte de verdade da baixa: nada e marcado como pago sem passar aqui.
        Task<ConsultaCobrancaPix> ConsultarCobrancaAsync(DadosBancariosEmpresa dados, string txid);

        // PATCH /cob/{txid} status REMOVIDA_PELO_USUARIO_RECEBEDOR: Pix gerado cuja mensagem
        // nao chegou a sair nao pode continuar pagavel.
        Task CancelarCobrancaAsync(DadosBancariosEmpresa dados, string txid);

        // PUT /webhook/{chave}.
        Task ConfigurarWebhookAsync(DadosBancariosEmpresa dados, string urlWebhook);

        // So obtem o token OAuth: confirma client id/secret/certificado sem gerar cobranca.
        Task TestarCredenciaisAsync(DadosBancariosEmpresa dados);

        // Pagina publica de pagamento (QR + copia e cola) e imagem do QR, usadas no botao e no
        // cabecalho do template.
        string LinkPagamento(string txid);
        string UrlQrCode(string txid);
        string UrlWebhook(Guid empresaId);
    }

    public record PixGerado(string Txid, string PixCopiaECola, string Status);

    // Horario ja convertido para Brasilia.
    public record ConsultaCobrancaPix(string Status, decimal? ValorPago, DateTime? PagoEm, string? EndToEndId)
    {
        public bool Paga => Status == "CONCLUIDA" && ValorPago.HasValue;
    }

    public class ItauPixException : Exception
    {
        public int? StatusHttp { get; }

        public ItauPixException(string mensagem, int? statusHttp = null, Exception? inner = null) : base(mensagem, inner)
        {
            StatusHttp = statusHttp;
        }
    }
}
