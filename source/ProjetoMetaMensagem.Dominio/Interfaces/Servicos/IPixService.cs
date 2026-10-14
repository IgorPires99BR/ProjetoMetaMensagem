using System;
using System.Threading.Tasks;

namespace ProjetoMetaMensagem.Dominio.Interfaces.Servicos
{
    // API do Asaas, na conta da PLATAFORMA (apiKey na configuracao, nao por empresa). O cliente
    // final paga a plataforma e o split repassa a parte da empresa para a SUBCONTA Asaas dela
    // (walletId em DadosBancariosEmpresa); a comissao fica na conta da plataforma.
    // Erros vem como PixException, com mensagem que pode ser mostrada ao usuario.
    public interface IPixService
    {
        // POST /customers (ou reaproveita o do mesmo CPF/CNPJ) + POST /payments com split +
        // GET /payments/{id}/pixQrCode.
        Task<PixGerado> CriarCobrancaAsync(NovaCobrancaPix cobranca);

        // GET /payments/{id}. Fonte de verdade da baixa: nada e marcado como pago sem passar aqui.
        Task<ConsultaCobrancaPix> ConsultarCobrancaAsync(string idExterno);

        // DELETE /payments/{id}: o QR do Asaas vale ate 12 meses depois do vencimento, entao
        // Pix cuja mensagem nao saiu (ou que passou da validade) precisa ser cancelado.
        Task CancelarCobrancaAsync(string idExterno);

        // POST /accounts. A apiKey da subconta so vem nesta resposta.
        Task<SubcontaCriada> CriarSubcontaAsync(NovaSubconta subconta);

        // Chamada leve (saldo): confirma a apiKey da plataforma sem gerar cobranca.
        Task TestarCredenciaisAsync();

        // Pagina publica de pagamento (QR + copia e cola) e imagem do QR, usadas no botao e no
        // cabecalho do template. Continuam nesta API (pelo txid), nao na fatura do Asaas.
        string LinkPagamento(string txid);
        string UrlQrCode(string txid);
    }

    // Quem paga: o Asaas exige nome e CPF/CNPJ para criar o customer. Telefone fica de fora: o
    // do contato e o do WhatsApp (com DDI, as vezes fixo) e o Asaas recusaria o customer por ele.
    public record PagadorPix(string Nome, string CpfCnpj, string? Email);

    // Repasse = parte da empresa (ValorCobrado - comissao), enviada para a subconta dela.
    // Txid vai no externalReference: e por ele que o webhook acha a CobrancaCliente.
    public record NovaCobrancaPix(string Txid, decimal Valor, DateTime Vencimento, string? Descricao,
                                  PagadorPix Pagador, string WalletIdRepasse, decimal ValorRepasse);

    public record PixGerado(string IdExterno, string PixCopiaECola, string Status);

    // Horario ja convertido para Brasilia.
    public record ConsultaCobrancaPix(string Status, decimal? ValorPago, DateTime? PagoEm, string? EndToEndId)
    {
        // CONFIRMED no Pix e o bloqueio cautelar de ate 72h do Asaas: ainda nao e dinheiro recebido.
        public bool Paga => Status == "RECEIVED" && ValorPago.HasValue;
    }

    // Dados exigidos pelo Asaas para abrir a subconta. DataNascimento so para CPF, TipoEmpresa
    // (MEI, LIMITED, INDIVIDUAL, ASSOCIATION) so para CNPJ.
    public record NovaSubconta(string Nome, string Email, string CpfCnpj, DateTime? DataNascimento, string? TipoEmpresa,
                               string Celular, decimal FaturamentoMensal, string Endereco, string Numero,
                               string? Complemento, string Bairro, string Cep);

    public record SubcontaCriada(string Id, string WalletId, string ApiKey);

    public class PixException : Exception
    {
        public int? StatusHttp { get; }

        public PixException(string mensagem, int? statusHttp = null, Exception? inner = null) : base(mensagem, inner)
        {
            StatusHttp = statusHttp;
        }
    }
}
