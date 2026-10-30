using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ProjetoMetaMensagem.Dominio.Interfaces.Servicos
{
    // API do Asaas, na conta da PLATAFORMA (apiKey na configuracao, nao por empresa). O cliente
    // final paga a plataforma e o split repassa a parte da empresa para a SUBCONTA Asaas dela
    // (walletId em DadosBancariosEmpresa); a comissao fica na conta da plataforma.
    // Erros vem como PixException, com mensagem que pode ser mostrada ao usuario.
    public interface IPixService
    {
        // POST /customers (ou reaproveita o do mesmo CPF/CNPJ) + POST /payments (com split se houver repasse) +
        // GET /payments/{id}/pixQrCode.
        Task<PixGerado> CriarCobrancaAsync(NovaCobrancaPix cobranca);

        // GET /payments/{id}. Fonte de verdade da baixa: nada e marcado como pago sem passar aqui.
        Task<ConsultaCobrancaPix> ConsultarCobrancaAsync(string idExterno);

        // DELETE /payments/{id}: o QR do Asaas vale ate 12 meses depois do vencimento, entao
        // Pix cuja mensagem nao saiu (ou que passou da validade) precisa ser cancelado.
        Task CancelarCobrancaAsync(string idExterno);

        // POST /accounts. A apiKey da subconta so vem nesta resposta.
        Task<SubcontaCriada> CriarSubcontaAsync(NovaSubconta subconta);

        // GET /accounts?walletId=. So lista subcontas da conta da plataforma: null = o walletId nao
        // e de uma subconta nossa (digitado errado ou de outra conta Asaas).
        Task<SubcontaExistente?> ObterSubcontaAsync(string walletId);

        // Cliente padrao da empresa (nome e CNPJ da subconta dela), usado quando o contato nao tem
        // CPF/CNPJ. O Asaas exige documento no customer, mas o QR pode ser pago por qualquer
        // pessoa -- a baixa casa pelo txid, nao pelo customer.
        Task<PagadorPix> PagadorPadraoDaSubcontaAsync(string walletId);

        // GET /myAccount/status + /myAccount/documents com a apiKey DA SUBCONTA (a da plataforma
        // devolveria a situacao da propria plataforma). Enquanto a subconta nao for aprovada o
        // split ate cai nela, mas o Asaas segura o saque: a empresa precisa enviar os documentos
        // pelos links (onboardingUrl) que vem aqui.
        Task<SituacaoSubconta> ConsultarSituacaoSubcontaAsync(string apiKeySubconta);

        // Asaas:Ambiente diferente de PRODUCAO.
        bool Sandbox { get; }

        // POST /sandbox/payment/{id}/confirm: paga a cobranca de mentira, para testar a baixa sem
        // dinheiro real. Devolve o status do Asaas depois da simulacao. Em producao lanca PixException.
        Task<string> ConfirmarPagamentoSandboxAsync(string idExterno);

        // Chamada leve (saldo): confirma a apiKey da plataforma sem gerar cobranca.
        Task TestarCredenciaisAsync();

        // Pagina publica de pagamento (QR + copia e cola) e imagem do QR, usadas no botao e no
        // cabecalho do template. Continuam nesta API (pelo txid), nao na fatura do Asaas.
        string LinkPagamento(string txid);
        string UrlQrCode(string txid);
    }

    // Quem paga: o Asaas exige nome e CPF/CNPJ para criar o customer; o Mercado Pago exige e-mail.
    // Telefone fica de fora: o do contato e o do WhatsApp (com DDI, as vezes fixo) e o Asaas
    // recusaria o customer por ele.
    public record PagadorPix(string Nome, string CpfCnpj, string? Email);

    // Asaas: Repasse = parte da empresa (ValorCobrado - comissao), enviada para a subconta dela.
    // Mercado Pago: TokenConta = conta da empresa, que cria a order; Comissao vai no marketplace_fee.
    // Sem nenhum dos dois o valor inteiro fica na conta da plataforma (ver DestinoRecebimento).
    // Txid vai no externalReference: e por ele que o webhook acha a CobrancaCliente.
    public record NovaCobrancaPix(string Txid, decimal Valor, DateTime Vencimento, string? Descricao,
                                  PagadorPix Pagador, string? WalletIdRepasse, decimal? ValorRepasse,
                                  string? TokenConta = null, decimal? Comissao = null);

    // LinkExterno: pagina de pagamento do proprio provedor (ticket_url do Mercado Pago), oferecida
    // na nossa pagina do Pix como alternativa.
    public record PixGerado(string IdExterno, string PixCopiaECola, string Status, string? LinkExterno = null);

    // Horario ja convertido para Brasilia. Status e o do provedor, sem traducao; os padroes de
    // PagaNoProvedor/AguardandoPagamento sao os do Asaas, os outros provedores informam os seus.
    public record ConsultaCobrancaPix(string Status, decimal? ValorPago, DateTime? PagoEm, string? EndToEndId,
                                      bool? PagaNoProvedor = null, bool? AguardandoPagamento = null)
    {
        // CONFIRMED no Pix e o bloqueio cautelar de ate 72h do Asaas: ainda nao e dinheiro recebido.
        public bool Paga => (PagaNoProvedor ?? Status == "RECEIVED") && ValorPago.HasValue;

        // Ainda pagavel: so assim a cobranca vencida e cancelada no provedor.
        public bool Pendente => AguardandoPagamento ?? Status is "PENDING" or "OVERDUE";
    }

    // Dados exigidos pelo Asaas para abrir a subconta. DataNascimento so para CPF, TipoEmpresa
    // (MEI, LIMITED, INDIVIDUAL, ASSOCIATION) so para CNPJ.
    public record NovaSubconta(string Nome, string Email, string CpfCnpj, DateTime? DataNascimento, string? TipoEmpresa,
                               string Celular, decimal FaturamentoMensal, string Endereco, string Numero,
                               string? Complemento, string Bairro, string Cep);

    public record SubcontaCriada(string Id, string WalletId, string ApiKey);

    // Status do Asaas sem traducao (PENDING, AWAITING_APPROVAL, APPROVED, REJECTED); a tela traduz.
    public record SituacaoSubconta(string Geral, string DadosComerciais, string DadosBancarios, string Documentacao,
                                   string? MotivoRecusa, IReadOnlyList<DocumentoSubconta> Documentos);

    // Grupo de documentos pedido pelo Asaas. Com LinkEnvio, o envio e so por ele (a API de
    // upload recusa); o link expira em LinkExpiraEm.
    public record DocumentoSubconta(string Titulo, string? Descricao, string Status, string? Responsavel,
                                    string? LinkEnvio, string? LinkExpiraEm);

    public record SubcontaExistente(string Id, string WalletId, string Nome, string CpfCnpj);

    public class PixException : Exception
    {
        public int? StatusHttp { get; }

        public PixException(string mensagem, int? statusHttp = null, Exception? inner = null) : base(mensagem, inner)
        {
            StatusHttp = statusHttp;
        }
    }
}
