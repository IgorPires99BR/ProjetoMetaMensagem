using Microsoft.Extensions.Logging;
using ProjetoMetaMensagem.Dominio.Entidades;
using ProjetoMetaMensagem.Dominio.Interfaces;
using ProjetoMetaMensagem.Dominio.Interfaces.Servicos;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace ProjetoMetaMensagem.Dominio.Servicos
{
    // A cobranca precisa existir ANTES do envio: o Pix (copia e cola, link da pagina, QR) vai
    // dentro da mensagem. A CobrancaCliente so e gravada depois que a Meta aceita o envio
    // (precisa do HistoricoDisparoId); ate la ela vive aqui, em memoria.
    public class CobrancaPreparada
    {
        public CobrancaCliente Cobranca { get; set; } = null!;

        // Preenchido quando a empresa tem cobranca Pix ativa mas o Pix deste contato nao saiu
        // (Itau fora, credencial errada, contato sem valor). O destinatario nao deve receber
        // uma cobranca sem forma de pagamento.
        public string? Erro { get; set; }

        public bool TemPix => Cobranca.Txid != null;
        public string? LinkPagamento { get; set; }
        public string? UrlQrCode { get; set; }
    }

    // Onde o Pix entra no template alem das variaveis do corpo: botao de URL com parte variavel
    // (pagina do Pix, sufixo = txid) e cabecalho de imagem (QR code). So preenche o que quem
    // disparou deixou em branco -- um cabecalho de logo informado na tela continua valendo.
    public static class PixNoTemplate
    {
        public static bool TemBotaoUrlDinamico(Template template) =>
            template.Componentes.Any(c => c.Tipo == Enums.TipoComponenteTemplate.Buttons
                && (c.Botoes ?? new()).Any(b => b.Tipo == Enums.TipoBotaoTemplate.Url && (b.Url ?? string.Empty).Contains("{{")));

        public static bool TemCabecalhoImagem(Template template) =>
            template.Componentes.Any(c => c.Tipo == Enums.TipoComponenteTemplate.Header && c.FormatMidia == Enums.TipoMidiaTemplate.Image);

        // Template de cobranca com botao de pagamento e sem Pix gerado: a Meta recusaria o envio
        // por falta do parametro do botao, com uma mensagem que ninguem entende. Barra antes.
        public static string? ErroDoBotaoSemPix(Template? template, CobrancaPreparada? preparada, bool botaoInformado)
        {
            if (template == null || !template.GeraCobranca || botaoInformado) return null;
            if (!TemBotaoUrlDinamico(template) || (preparada != null && preparada.TemPix)) return null;

            return "O template tem botão de pagamento Pix, mas a empresa não tem cobrança Pix ativa nos dados bancários.";
        }
    }

    public interface IPreparadorCobrancaPix
    {
        Task<Dictionary<Guid, CobrancaPreparada>> PrepararAsync(Guid empresaId, Template template, IEnumerable<Contato> contatos, DateTime agora);

        // Cancela no Itau o Pix das cobrancas que nao foram enviadas. Nunca lanca excecao.
        Task DescartarAsync(Guid empresaId, IEnumerable<CobrancaPreparada> naoEnviadas);

        // Troca os marcadores de Pix (ver ResolvedorDeVariaveis) pelos valores desta cobranca.
        // Devolve erro quando o template pede Pix e a cobranca nao tem.
        string? AplicarNasVariaveis(List<string> valores, CobrancaPreparada? preparada);
    }

    public class PreparadorCobrancaPix : IPreparadorCobrancaPix
    {
        // Lote de centenas de contatos nao pode abrir centenas de conexoes simultaneas no Itau.
        private const int ChamadasSimultaneas = 5;

        private readonly IUnitOfWork _unitOfWork;
        private readonly IItauPixService _itau;
        private readonly ILogger<PreparadorCobrancaPix> _logger;

        public PreparadorCobrancaPix(IUnitOfWork unitOfWork, IItauPixService itau, ILogger<PreparadorCobrancaPix> logger)
        {
            _unitOfWork = unitOfWork;
            _itau = itau;
            _logger = logger;
        }

        public async Task<Dictionary<Guid, CobrancaPreparada>> PrepararAsync(Guid empresaId, Template template, IEnumerable<Contato> contatos, DateTime agora)
        {
            var preparadas = new Dictionary<Guid, CobrancaPreparada>();
            foreach (var contato in contatos)
            {
                if (preparadas.ContainsKey(contato.Id)) continue;

                // HistoricoDisparoId fica vazio ate o envio dar certo.
                var cobranca = CobrancaClienteFactory.Criar(contato, template.Id, Guid.Empty, agora);
                var calculo = CalculoCobrancaPix.Calcular(cobranca.Valor, contato.TaxaJuros, contato.TaxaJurosMensal, cobranca.DataVencimento, agora);
                cobranca.Multa = calculo.Multa;
                cobranca.Juros = calculo.Juros;
                cobranca.ValorCobrado = calculo.ValorCobrado;

                preparadas[contato.Id] = new CobrancaPreparada { Cobranca = cobranca };
            }

            var dados = await _unitOfWork.DadosBancariosEmpresa.ObterPorEmpresa(empresaId);
            if (dados == null || !dados.CobrancaPixAtiva) return preparadas;

            using var limite = new SemaphoreSlim(ChamadasSimultaneas);
            var contatoPorId = contatos.GroupBy(c => c.Id).ToDictionary(g => g.Key, g => g.First());

            await Task.WhenAll(preparadas.Values.Select(async preparada =>
            {
                await limite.WaitAsync();
                try
                {
                    await GerarPix(dados, preparada, contatoPorId[preparada.Cobranca.ContatoId], agora);
                }
                finally
                {
                    limite.Release();
                }
            }));

            return preparadas;
        }

        private async Task GerarPix(DadosBancariosEmpresa dados, CobrancaPreparada preparada, Contato contato, DateTime agora)
        {
            var cobranca = preparada.Cobranca;
            if (cobranca.ValorCobrado is not > 0m)
            {
                preparada.Erro = "Contato sem valor de fatura cadastrado: não dá para gerar o Pix.";
                return;
            }

            var txid = CobrancaCliente.TxidDe(cobranca.Id);
            var expiraEm = CalculoCobrancaPix.Calcular(cobranca.Valor, contato.TaxaJuros, contato.TaxaJurosMensal, cobranca.DataVencimento, agora).ExpiraEm;
            // Minimo de 1 hora: disparo feito as 23:50 de um dia vencido nao pode sair com um Pix
            // que expira em 10 minutos.
            var segundos = Math.Max(3600, (int)(expiraEm - agora).TotalSeconds);

            var nome = string.IsNullOrWhiteSpace(contato.NomeCliente) ? contato.NomeContato : contato.NomeCliente;
            var solicitacao = $"Fatura{(string.IsNullOrWhiteSpace(nome) ? "" : " de " + nome)} - venc. {cobranca.DataVencimento.ToString("dd/MM/yyyy", CultureInfo.InvariantCulture)}";

            try
            {
                var pix = await _itau.CriarCobrancaAsync(dados, txid, cobranca.ValorCobrado.Value, segundos, solicitacao);
                cobranca.Txid = txid;
                cobranca.PixCopiaECola = pix.PixCopiaECola;
                cobranca.PixExpiraEm = agora.AddSeconds(segundos);
                preparada.LinkPagamento = _itau.LinkPagamento(txid);
                preparada.UrlQrCode = _itau.UrlQrCode(txid);
            }
            catch (ItauPixException ex)
            {
                preparada.Erro = $"Pix não gerado: {ex.Message}";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "ItauPix: erro inesperado ao gerar Pix do contato {ContatoId}", contato.Id);
                preparada.Erro = "Pix não gerado: erro inesperado ao falar com o Itaú.";
            }
        }

        public async Task DescartarAsync(Guid empresaId, IEnumerable<CobrancaPreparada> naoEnviadas)
        {
            var comPix = naoEnviadas.Where(p => p.TemPix).ToList();
            if (comPix.Count == 0) return;

            try
            {
                var dados = await _unitOfWork.DadosBancariosEmpresa.ObterPorEmpresa(empresaId);
                if (dados == null) return;

                foreach (var preparada in comPix)
                {
                    try
                    {
                        await _itau.CancelarCobrancaAsync(dados, preparada.Cobranca.Txid!);
                    }
                    catch (Exception ex)
                    {
                        // Sem cancelar, o Pix so expira sozinho no prazo -- e ninguem recebeu o
                        // codigo, entao o risco e baixo. Fica o log.
                        _logger.LogWarning(ex, "ItauPix: nao foi possivel cancelar o Pix {Txid} de mensagem nao enviada", preparada.Cobranca.Txid);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "ItauPix: falha ao descartar Pix nao enviados da empresa {EmpresaId}", empresaId);
            }
        }

        public string? AplicarNasVariaveis(List<string> valores, CobrancaPreparada? preparada)
        {
            for (var i = 0; i < valores.Count; i++)
            {
                var valor = valores[i];
                if (valor != ResolvedorDeVariaveis.MarcadorPixCopiaECola && valor != ResolvedorDeVariaveis.MarcadorLinkPagamento)
                    continue;

                if (preparada == null)
                    return "O template usa variável de Pix, mas não está marcado para gerar cobrança.";

                if (!preparada.TemPix)
                    return preparada.Erro
                        ?? "O template usa variável de Pix, mas a empresa não tem cobrança Pix ativa nos dados bancários.";

                valores[i] = valor == ResolvedorDeVariaveis.MarcadorPixCopiaECola
                    ? preparada.Cobranca.PixCopiaECola!
                    : preparada.LinkPagamento!;
            }

            return null;
        }
    }
}
