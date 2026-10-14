using Microsoft.Extensions.Logging;
using ProjetoMetaMensagem.Dominio.Entidades;
using ProjetoMetaMensagem.Dominio.Interfaces;
using ProjetoMetaMensagem.Dominio.Interfaces.Servicos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ProjetoMetaMensagem.Dominio.Servicos
{
    // Baixa automatica das cobrancas pagas por Pix. Dois caminhos chegam aqui: o webhook do
    // Asaas (rapido) e a consulta periodica (rede de seguranca: fila de webhook pausada, deploy
    // no meio do aviso). Nos dois, o pagamento so e aceito depois de reconsultar o Asaas: o corpo
    // do webhook nao basta (o endpoint e publico e o txid aparece no link enviado ao cliente).
    //
    // Pago = so dar baixa. Nenhuma mensagem de confirmacao e enviada no WhatsApp (decisao do Igor).
    public interface IBaixaCobrancaPix
    {
        Task ProcessarWebhookAsync(IEnumerable<string> txids);
        Task ConsultarPendentesAsync();
    }

    public class BaixaCobrancaPix : IBaixaCobrancaPix
    {
        // Janela em que a consulta periodica ainda olha um Pix depois da validade.
        private static readonly TimeSpan ToleranciaAposExpirar = TimeSpan.FromHours(2);

        // O QR do Asaas continua pagavel por meses depois do vencimento, com o valor do dia do
        // envio (juros parados). Passou disso sem pagamento, a cobranca e cancelada no Asaas.
        // Folga de 30 min para nao cancelar um Pix pago no ultimo minuto e ainda nao liquidado.
        private static readonly TimeSpan CancelarAposExpirar = TimeSpan.FromMinutes(30);

        private readonly IUnitOfWork _unitOfWork;
        private readonly IPixService _pix;
        private readonly INotificadorCobranca _notificador;
        private readonly ILogger<BaixaCobrancaPix> _logger;

        public BaixaCobrancaPix(IUnitOfWork unitOfWork, IPixService pix, INotificadorCobranca notificador, ILogger<BaixaCobrancaPix> logger)
        {
            _unitOfWork = unitOfWork;
            _pix = pix;
            _notificador = notificador;
            _logger = logger;
        }

        public async Task ProcessarWebhookAsync(IEnumerable<string> txids)
        {
            foreach (var txid in txids.Where(t => !string.IsNullOrWhiteSpace(t)).Distinct())
            {
                var cobranca = await _unitOfWork.CobrancaCliente.ObterPorTxid(txid);
                // Cobranca da conta da plataforma que nao e nossa (criada no painel): ignora.
                if (cobranca == null) continue;

                await ConferirAsync(cobranca, podeCancelar: false);
            }
        }

        public async Task ConsultarPendentesAsync()
        {
            var pendentes = (await _unitOfWork.CobrancaCliente.ObterPendentesComPix(HorarioBrasilia.Agora() - ToleranciaAposExpirar)).ToList();
            if (pendentes.Count == 0) return;

            var baixadas = 0;
            foreach (var cobranca in pendentes)
            {
                if (await ConferirAsync(cobranca, podeCancelar: true)) baixadas++;
            }

            _logger.LogInformation("AsaasPix: consulta periodica conferiu {Total} cobranca(s) pendente(s), {Baixadas} baixada(s).", pendentes.Count, baixadas);
        }

        private async Task<bool> ConferirAsync(CobrancaCliente cobranca, bool podeCancelar)
        {
            // Sem PixIdExterno = Pix da Transfeera/Itau, que o Asaas nao conhece.
            if (cobranca.Status != StatusCobrancaCliente.Pendente || string.IsNullOrEmpty(cobranca.PixIdExterno)) return false;

            try
            {
                var consulta = await _pix.ConsultarCobrancaAsync(cobranca.PixIdExterno);
                if (!consulta.Paga)
                {
                    if (podeCancelar && cobranca.PixExpiraEm.HasValue && HorarioBrasilia.Agora() > cobranca.PixExpiraEm.Value + CancelarAposExpirar
                        && consulta.Status is "PENDING" or "OVERDUE")
                    {
                        await _pix.CancelarCobrancaAsync(cobranca.PixIdExterno);
                        _logger.LogInformation("AsaasPix: cobranca {CobrancaId} expirada sem pagamento, cancelada no Asaas ({IdExterno}).",
                            cobranca.Id, cobranca.PixIdExterno);
                    }
                    return false;
                }

                var alteradas = await _unitOfWork.CobrancaCliente.BaixarPix(
                    cobranca.Id, consulta.PagoEm ?? HorarioBrasilia.Agora(), consulta.ValorPago!.Value, consulta.EndToEndId);

                if (alteradas > 0)
                {
                    _logger.LogInformation("AsaasPix: cobranca {CobrancaId} paga ({IdExterno}, R$ {Valor}).",
                        cobranca.Id, cobranca.PixIdExterno, consulta.ValorPago);
                    await _notificador.NotificarCobrancaAtualizadaAsync(cobranca.EmpresaId, cobranca.Id, StatusCobrancaCliente.Paga);
                }
                return alteradas > 0;
            }
            catch (Exception ex)
            {
                // Uma cobranca com problema nao pode parar a conferencia das outras; a proxima
                // rodada tenta de novo.
                _logger.LogWarning(ex, "AsaasPix: falha ao conferir a cobranca {CobrancaId} ({IdExterno}).", cobranca.Id, cobranca.PixIdExterno);
                return false;
            }
        }
    }
}
