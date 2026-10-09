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
    // Itau (rapido, mas sem garantia de entrega -- ver ItauPixConfiguration) e a consulta
    // periodica. Nos dois, o pagamento so e aceito depois de reconsultar o Itau com as
    // credenciais da propria empresa: o corpo do webhook nao e confiavel (o endpoint e publico
    // e o txid aparece no link enviado ao cliente).
    //
    // Pago = so dar baixa. Nenhuma mensagem de confirmacao e enviada no WhatsApp (decisao do Igor).
    public interface IBaixaCobrancaPix
    {
        Task ProcessarWebhookAsync(Guid empresaId, IEnumerable<string> txids);
        Task ConsultarPendentesAsync();
    }

    public class BaixaCobrancaPix : IBaixaCobrancaPix
    {
        // Pagamento feito nos ultimos minutos de validade pode ser liquidado depois dela.
        private static readonly TimeSpan ToleranciaAposExpirar = TimeSpan.FromHours(2);

        private readonly IUnitOfWork _unitOfWork;
        private readonly IItauPixService _itau;
        private readonly ILogger<BaixaCobrancaPix> _logger;

        public BaixaCobrancaPix(IUnitOfWork unitOfWork, IItauPixService itau, ILogger<BaixaCobrancaPix> logger)
        {
            _unitOfWork = unitOfWork;
            _itau = itau;
            _logger = logger;
        }

        public async Task ProcessarWebhookAsync(Guid empresaId, IEnumerable<string> txids)
        {
            var dados = await _unitOfWork.DadosBancariosEmpresa.ObterPorEmpresa(empresaId);
            if (dados == null) return;

            foreach (var txid in txids.Where(t => !string.IsNullOrWhiteSpace(t)).Distinct())
            {
                var cobranca = await _unitOfWork.CobrancaCliente.ObterPorTxid(txid);
                // Pix de outra empresa (ou recebido na chave sem ser cobranca nossa): ignora.
                if (cobranca == null || cobranca.EmpresaId != empresaId) continue;

                await ConferirAsync(cobranca, dados);
            }
        }

        public async Task ConsultarPendentesAsync()
        {
            var pendentes = (await _unitOfWork.CobrancaCliente.ObterPendentesComPix(HorarioBrasilia.Agora() - ToleranciaAposExpirar)).ToList();
            if (pendentes.Count == 0) return;

            var baixadas = 0;
            foreach (var porEmpresa in pendentes.GroupBy(c => c.EmpresaId))
            {
                var dados = await _unitOfWork.DadosBancariosEmpresa.ObterPorEmpresa(porEmpresa.Key);
                if (dados == null) continue;

                foreach (var cobranca in porEmpresa)
                {
                    if (await ConferirAsync(cobranca, dados)) baixadas++;
                }
            }

            _logger.LogInformation("ItauPix: consulta periodica conferiu {Total} cobranca(s) pendente(s), {Baixadas} baixada(s).", pendentes.Count, baixadas);
        }

        private async Task<bool> ConferirAsync(CobrancaCliente cobranca, DadosBancariosEmpresa dados)
        {
            if (cobranca.Status != StatusCobrancaCliente.Pendente || string.IsNullOrEmpty(cobranca.Txid)) return false;

            try
            {
                var consulta = await _itau.ConsultarCobrancaAsync(dados, cobranca.Txid);
                if (!consulta.Paga) return false;

                var alteradas = await _unitOfWork.CobrancaCliente.BaixarPix(
                    cobranca.Id, consulta.PagoEm ?? HorarioBrasilia.Agora(), consulta.ValorPago!.Value, consulta.EndToEndId);

                if (alteradas > 0)
                {
                    _logger.LogInformation("ItauPix: cobranca {CobrancaId} paga (txid {Txid}, R$ {Valor}).",
                        cobranca.Id, cobranca.Txid, consulta.ValorPago);
                }
                return alteradas > 0;
            }
            catch (Exception ex)
            {
                // Uma cobranca com problema nao pode parar a conferencia das outras; a proxima
                // rodada tenta de novo.
                _logger.LogWarning(ex, "ItauPix: falha ao conferir a cobranca {CobrancaId} (txid {Txid}).", cobranca.Id, cobranca.Txid);
                return false;
            }
        }
    }
}
