using Microsoft.Extensions.Logging;
using ProjetoMetaMensagem.Dominio.Entidades;
using ProjetoMetaMensagem.Dominio.Helpers;
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
        // (Asaas fora, comissao maior que a fatura, contato sem valor). O destinatario nao
        // deve receber uma cobranca sem forma de pagamento.
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

        // Cancela no Asaas o Pix das cobrancas que nao foram enviadas. Nunca lanca excecao.
        Task DescartarAsync(Guid empresaId, IEnumerable<CobrancaPreparada> naoEnviadas);

        // Troca os marcadores de Pix (ver ResolvedorDeVariaveis) pelos valores desta cobranca.
        // Devolve erro quando o template pede Pix e a cobranca nao tem.
        string? AplicarNasVariaveis(List<string> valores, CobrancaPreparada? preparada);
    }

    public class PreparadorCobrancaPix : IPreparadorCobrancaPix
    {
        // Lote de centenas de contatos nao pode abrir centenas de conexoes simultaneas no provedor
        // (no Asaas cada Pix sao 2 a 4 chamadas: cliente, cobranca e QR).
        private const int ChamadasSimultaneas = 5;

        private readonly IUnitOfWork _unitOfWork;
        // Asaas: cliente padrao da subconta e os links da nossa pagina do Pix (que servem a
        // qualquer provedor). Quem gera o Pix e o provedor ativo.
        private readonly IPixService _pix;
        private readonly IProvedoresCobranca _provedores;
        private readonly ILogger<PreparadorCobrancaPix> _logger;

        public PreparadorCobrancaPix(IUnitOfWork unitOfWork, IPixService pix, IProvedoresCobranca provedores, ILogger<PreparadorCobrancaPix> logger)
        {
            _unitOfWork = unitOfWork;
            _pix = pix;
            _provedores = provedores;
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

            IProvedorCobranca provedor;
            try
            {
                provedor = _provedores.Ativo;
            }
            catch (PixException ex)
            {
                foreach (var preparada in preparadas.Values) preparada.Erro = $"Pix não gerado: {ex.Message}";
                return preparadas;
            }

            // Split so no Asaas e quando a empresa nao recebe pela conta da plataforma.
            var comSplit = provedor.SuportaSplit && !dados.RecebimentoNaPlataforma;

            // Conta Asaas propria da empresa: o GET /accounts da plataforma nao a enxerga, entao o
            // cliente padrao sai do cadastro da empresa. Sem split tambem: pode nem existir
            // subconta para consultar.
            PagadorPix? pagadorExterno = null;
            if (provedor.SuportaSplit && (dados.CarteiraExterna || !comSplit))
            {
                var empresa = await _unitOfWork.Empresa.ObterPorId(empresaId);
                if (empresa != null && DocumentoHelper.CpfCnpjValido(empresa.Cnpj))
                    pagadorExterno = new PagadorPix($"Clientes {empresa.Nome}", DocumentoHelper.SoDigitos(empresa.Cnpj)!, null);
            }

            using var limite = new SemaphoreSlim(ChamadasSimultaneas);
            var contatoPorId = contatos.GroupBy(c => c.Id).ToDictionary(g => g.Key, g => g.First());

            await Task.WhenAll(preparadas.Values.Select(async preparada =>
            {
                await limite.WaitAsync();
                try
                {
                    await GerarPix(provedor, comSplit, dados, pagadorExterno, preparada, contatoPorId[preparada.Cobranca.ContatoId], agora);
                }
                finally
                {
                    limite.Release();
                }
            }));

            return preparadas;
        }

        private async Task GerarPix(IProvedorCobranca provedor, bool comSplit, DadosBancariosEmpresa dados, PagadorPix? pagadorExterno,
                                    CobrancaPreparada preparada, Contato contato, DateTime agora)
        {
            var cobranca = preparada.Cobranca;
            var faltando = FaltaDadoDeCobranca(contato);
            if (faltando != null)
            {
                preparada.Erro = $"Pix não gerado: o cadastro do contato está sem {faltando}, obrigatório para gerar a cobrança Pix.";
                return;
            }

            if (cobranca.ValorCobrado is not > 0m)
            {
                preparada.Erro = "Contato sem valor de fatura cadastrado: não dá para gerar o Pix.";
                return;
            }

            CalculoComissao.Resultado? divisao = null;
            if (comSplit)
            {
                // A ativacao ja exige os dois, mas o registro pode ter sido mexido direto no banco.
                if (!dados.TemSubconta || !dados.ComissaoDefinida)
                {
                    preparada.Erro = "Pix não gerado: a empresa está sem subconta Asaas ou sem comissão definida nos dados bancários.";
                    return;
                }

                divisao = CalculoComissao.Calcular(cobranca.ValorCobrado.Value, dados.ComissaoTipo!, dados.ComissaoValor!.Value);
                if (divisao == null)
                {
                    preparada.Erro = "Pix não gerado: o valor da cobrança não cobre a comissão da plataforma.";
                    return;
                }
            }

            var txid = CobrancaCliente.TxidDe(cobranca.Id);
            var expiraEm = CalculoCobrancaPix.Calcular(cobranca.Valor, contato.TaxaJuros, contato.TaxaJurosMensal, cobranca.DataVencimento, agora).ExpiraEm;
            // Minimo de 1 hora: disparo feito as 23:50 de um dia vencido nao pode sair com um Pix
            // que expira em 10 minutos.
            // O QR do Asaas continua pagavel depois do vencimento; quem faz valer esta validade e
            // a BaixaCobrancaPix, cancelando no Asaas o que passou dela. A order do Mercado Pago
            // expira sozinha (expiration_time).
            if (expiraEm < agora.AddHours(1)) expiraEm = agora.AddHours(1);

            var nome = string.IsNullOrWhiteSpace(contato.NomeCliente) ? contato.NomeContato : contato.NomeCliente;
            var descricao = $"Fatura{(string.IsNullOrWhiteSpace(nome) ? "" : " de " + nome)} - venc. {cobranca.DataVencimento.ToString("dd/MM/yyyy", CultureInfo.InvariantCulture)}";
            try
            {
                // Contato sem documento (a Sebrecon nao tem o dos clientes) vai para o cliente
                // padrao da empresa; o nome dele continua na descricao da cobranca.
                var nomePagador = string.IsNullOrWhiteSpace(nome) ? contato.Telefone : nome!;
                var pagador = DocumentoHelper.CpfCnpjValido(contato.CpfCnpj)
                    ? new PagadorPix(nomePagador, contato.CpfCnpj!, contato.Email)
                    // O Mercado Pago nao exige documento do pagador, so e-mail (com fallback no
                    // servico): nada de cliente padrao.
                    : !provedor.SuportaSplit
                        ? new PagadorPix(nomePagador, string.Empty, contato.Email)
                        : dados.CarteiraExterna || !comSplit
                            ? pagadorExterno ?? throw new PixException("a empresa não tem CNPJ válido no cadastro para usar como cliente padrão.")
                            : await _pix.PagadorPadraoDaSubcontaAsync(dados.AsaasWalletId!);

                var pix = await provedor.CriarCobrancaAsync(new NovaCobrancaPix(
                    txid, cobranca.ValorCobrado.Value, expiraEm.Date, descricao,
                    pagador, divisao == null ? null : dados.AsaasWalletId, divisao?.Repasse));
                cobranca.Txid = txid;
                cobranca.PixIdExterno = pix.IdExterno;
                cobranca.PixCopiaECola = pix.PixCopiaECola;
                cobranca.PixExpiraEm = expiraEm;
                cobranca.ValorComissao = divisao?.Comissao;
                cobranca.ValorRepasse = divisao?.Repasse;
                cobranca.RecebidaNaPlataforma = divisao == null;
                cobranca.Provedor = provedor.Nome;
                cobranca.LinkPagamentoExterno = pix.LinkExterno;
                preparada.LinkPagamento = _pix.LinkPagamento(txid);
                preparada.UrlQrCode = _pix.UrlQrCode(txid);
            }
            catch (PixException ex)
            {
                preparada.Erro = $"Pix não gerado: {ex.Message}";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Pix {Provedor}: erro inesperado ao gerar Pix do contato {ContatoId}", provedor.Nome, contato.Id);
                preparada.Erro = "Pix não gerado: erro inesperado ao falar com o provedor de pagamento.";
            }
        }

        // Valor, vencimento e juros sao opcionais no cadastro do contato, mas sem eles o Pix sairia
        // com vencimento "hoje" e sem multa/juros, que ninguem combinou com o cliente. Zero vale
        // (contrato sem juros); so o campo em branco barra.
        public static string? FaltaDadoDeCobranca(Contato contato)
        {
            var faltando = new List<string>();
            if (!contato.ValorFatura.HasValue) faltando.Add("valor da fatura");
            if (!contato.DiaVencimento.HasValue) faltando.Add("dia de vencimento");
            if (!contato.TaxaJuros.HasValue) faltando.Add("taxa de juros (multa)");
            if (!contato.TaxaJurosMensal.HasValue) faltando.Add("juros ao mês");

            return faltando.Count == 0 ? null : string.Join(", ", faltando);
        }

        public async Task DescartarAsync(Guid empresaId, IEnumerable<CobrancaPreparada> naoEnviadas)
        {
            foreach (var preparada in naoEnviadas.Where(p => p.TemPix && p.Cobranca.PixIdExterno != null))
            {
                try
                {
                    await _provedores.Do(preparada.Cobranca.Provedor).CancelarCobrancaAsync(preparada.Cobranca.PixIdExterno!);
                }
                catch (Exception ex)
                {
                    // A CobrancaCliente nao e gravada, entao a consulta periodica nao volta a
                    // esse Pix: fica so no provedor, pendente. Ninguem recebeu o codigo, entao o
                    // risco e baixo; o log da o id para cancelar pelo painel.
                    _logger.LogWarning(ex, "Pix {Provedor}: nao foi possivel cancelar a cobranca {IdExterno} (txid {Txid}) de mensagem nao enviada (empresa {EmpresaId})",
                        preparada.Cobranca.Provedor, preparada.Cobranca.PixIdExterno, preparada.Cobranca.Txid, empresaId);
                }
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
