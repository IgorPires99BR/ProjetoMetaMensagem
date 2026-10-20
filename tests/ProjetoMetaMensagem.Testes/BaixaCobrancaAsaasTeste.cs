using Microsoft.Extensions.Logging.Abstractions;
using ProjetoMetaMensagem.Dominio.Entidades;
using ProjetoMetaMensagem.Dominio.Interfaces;
using ProjetoMetaMensagem.Dominio.Interfaces.Repositorios;
using ProjetoMetaMensagem.Dominio.Interfaces.Servicos;
using ProjetoMetaMensagem.Dominio.Servicos;
using Xunit;

namespace ProjetoMetaMensagem.Testes;

// Cliente paga o Pix -> webhook do Asaas -> reconsulta -> cobranca PAGA -> tela avisada em
// tempo real. O corpo do webhook nunca basta: so a consulta ao Asaas da baixa.
public class BaixaCobrancaAsaasTeste
{
    [Fact]
    public async Task Webhook_de_Pix_pago_da_baixa_e_avisa_a_tela_da_empresa()
    {
        var cenario = new Cenario(statusNoAsaas: "RECEIVED");

        await cenario.Baixa.ProcessarWebhookAsync(new[] { cenario.Cobranca.Txid! });

        Assert.Equal(StatusCobrancaCliente.Paga, cenario.Cobranca.Status);
        Assert.Equal(405m, cenario.Cobranca.ValorPago);
        var aviso = Assert.Single(cenario.Notificador.Avisos);
        Assert.Equal((cenario.Cobranca.EmpresaId, cenario.Cobranca.Id, StatusCobrancaCliente.Paga), aviso);
    }

    [Fact]
    public async Task Webhook_sem_pagamento_confirmado_no_Asaas_nao_da_baixa()
    {
        var cenario = new Cenario(statusNoAsaas: "PENDING");

        await cenario.Baixa.ProcessarWebhookAsync(new[] { cenario.Cobranca.Txid! });

        Assert.Equal(StatusCobrancaCliente.Pendente, cenario.Cobranca.Status);
        Assert.Empty(cenario.Notificador.Avisos);
    }

    [Fact]
    public async Task Txid_desconhecido_e_ignorado()
    {
        var cenario = new Cenario(statusNoAsaas: "RECEIVED");

        await cenario.Baixa.ProcessarWebhookAsync(new[] { "outrotxid" });

        Assert.Equal(StatusCobrancaCliente.Pendente, cenario.Cobranca.Status);
        Assert.Empty(cenario.Pix.Consultas);
    }

    [Fact]
    public async Task Consulta_periodica_cancela_no_Asaas_o_Pix_que_passou_da_validade()
    {
        var cenario = new Cenario(statusNoAsaas: "OVERDUE");
        cenario.Cobranca.PixExpiraEm = HorarioBrasilia.Agora().AddHours(-1);

        await cenario.Baixa.ConsultarPendentesAsync();

        Assert.Equal(new[] { cenario.Cobranca.PixIdExterno }, cenario.Pix.Canceladas);
        Assert.Equal(StatusCobrancaCliente.Pendente, cenario.Cobranca.Status);
    }

    [Fact]
    public async Task Contato_sem_CpfCnpj_gera_Pix_no_cliente_padrao_da_subconta()
    {
        var (pix, preparada) = await PrepararPara(cpfCnpj: null);

        Assert.True(preparada.TemPix, preparada.Erro);
        Assert.Equal(new PagadorPix("Clientes Sebrecon", "19183412000132", null), pix.Criadas.Single().Pagador);
        Assert.Equal("wal_sebrecon", pix.Criadas.Single().WalletIdRepasse);
    }

    [Fact]
    public async Task Contato_com_CpfCnpj_continua_sendo_o_cliente_da_cobranca()
    {
        var (pix, _) = await PrepararPara(cpfCnpj: "52998224725");

        Assert.Equal("52998224725", pix.Criadas.Single().Pagador.CpfCnpj);
        Assert.Equal(0, pix.ConsultasDeSubconta);
    }

    [Fact]
    public async Task Conta_Asaas_propria_usa_o_CNPJ_da_empresa_como_cliente_padrao()
    {
        // Carteira externa: o GET /accounts da plataforma nao a enxerga.
        var (pix, preparada) = await PrepararPara(cpfCnpj: null, carteiraExterna: true);

        Assert.True(preparada.TemPix, preparada.Erro);
        Assert.Equal(new PagadorPix("Clientes Sebrecon", "19183412000132", null), pix.Criadas.Single().Pagador);
        Assert.Equal("wal_sebrecon", pix.Criadas.Single().WalletIdRepasse);
        Assert.Equal(0, pix.ConsultasDeSubconta);
    }

    [Fact]
    public async Task Conta_Asaas_propria_sem_CNPJ_na_empresa_nao_gera_Pix()
    {
        var (pix, preparada) = await PrepararPara(cpfCnpj: null, carteiraExterna: true, cnpjEmpresa: "");

        Assert.False(preparada.TemPix);
        Assert.Contains("CNPJ", preparada.Erro);
        Assert.Empty(pix.Criadas);
    }

    [Fact]
    public async Task Contato_sem_dados_de_cobranca_nao_gera_Pix()
    {
        var (pix, preparada) = await PrepararPara(cpfCnpj: "52998224725", semJuros: true);

        Assert.False(preparada.TemPix);
        Assert.Contains("juros", preparada.Erro);
        Assert.Empty(pix.Criadas);
    }

    private static async Task<(PixFalso, CobrancaPreparada)> PrepararPara(string? cpfCnpj, bool carteiraExterna = false,
        string cnpjEmpresa = "19.183.412/0001-32", bool semJuros = false)
    {
        var empresaId = Guid.NewGuid();
        var contato = new Contato
        {
            Id = Guid.NewGuid(), EmpresaId = empresaId, Telefone = "5511999990000",
            NomeCliente = "Maria Souza", DiaVencimento = 20, ValorFatura = 405m, CpfCnpj = cpfCnpj,
            TaxaJuros = semJuros ? null : 2m, TaxaJurosMensal = semJuros ? null : 1m
        };
        var dados = new DadosBancariosEmpresa
        {
            EmpresaId = empresaId, CobrancaPixAtiva = true, AsaasWalletId = "wal_sebrecon",
            AsaasContaId = carteiraExterna ? null : "acc_sebrecon",
            ComissaoTipo = DadosBancariosEmpresa.ComissaoFixa, ComissaoValor = 3.50m
        };
        var pix = new PixFalso("PENDING");
        var uow = new UnitOfWorkFalso(new CobrancaRepositorioFalso(new CobrancaCliente()), new DadosBancariosFalso(dados),
            new EmpresaFalsa(new Empresa { Id = empresaId, Nome = "Sebrecon", Cnpj = cnpjEmpresa }));

        var preparadas = await new PreparadorCobrancaPix(uow, pix, NullLogger<PreparadorCobrancaPix>.Instance)
            .PrepararAsync(empresaId, new Template { Id = Guid.NewGuid() }, new[] { contato }, new DateTime(2026, 10, 14, 10, 0, 0));

        return (pix, preparadas[contato.Id]);
    }

    private class EmpresaFalsa : IEmpresaRepository
    {
        private readonly Empresa _empresa;
        public EmpresaFalsa(Empresa empresa) => _empresa = empresa;
        public Task<Empresa?> ObterPorId(Guid id) => Task.FromResult<Empresa?>(id == _empresa.Id ? _empresa : null);
        public Task<Guid> Incluir(Empresa empresa) => throw new NotSupportedException();
        public Task<Guid> Alterar(Empresa empresa) => throw new NotSupportedException();
        public Task<int> Excluir(string id) => throw new NotSupportedException();
        public Task<string?> ObterMetaAccessToken(Guid id) => throw new NotSupportedException();
        public Task<string?> ObterWabaId(Guid id) => throw new NotSupportedException();
        public Task<string?> ObterPhoneNumberId(Guid id) => throw new NotSupportedException();
        public Task<string?> ObterAppIdMeta(Guid id) => throw new NotSupportedException();
        public Task<Guid?> ObterPorPhoneNumberId(string phoneNumberId) => throw new NotSupportedException();
        public Task AtualizarWabaId(Guid id, string wabaId) => throw new NotSupportedException();
        public Task AtualizarCredenciaisMeta(Guid id, string? wabaId, string? phoneNumberId, string? metaAccessToken) => throw new NotSupportedException();
        public Task<List<Empresa>> Obter() => throw new NotSupportedException();
    }

    private class DadosBancariosFalso : IDadosBancariosEmpresaRepository
    {
        private readonly DadosBancariosEmpresa _dados;
        public DadosBancariosFalso(DadosBancariosEmpresa dados) => _dados = dados;
        public Task<DadosBancariosEmpresa?> ObterPorEmpresa(Guid empresaId) => Task.FromResult<DadosBancariosEmpresa?>(_dados);
        public Task Salvar(DadosBancariosEmpresa dados) => throw new NotSupportedException();
    }

    private class Cenario
    {
        public CobrancaCliente Cobranca { get; }
        public PixFalso Pix { get; }
        public NotificadorFalso Notificador { get; } = new();
        public BaixaCobrancaPix Baixa { get; }

        public Cenario(string statusNoAsaas)
        {
            Cobranca = new CobrancaCliente
            {
                EmpresaId = Guid.NewGuid(),
                Valor = 405m,
                ValorCobrado = 405m,
                PixIdExterno = "pay_123",
                PixExpiraEm = HorarioBrasilia.Agora().AddHours(5)
            };
            Cobranca.Txid = CobrancaCliente.TxidDe(Cobranca.Id);

            Pix = new PixFalso(statusNoAsaas);
            Baixa = new BaixaCobrancaPix(new UnitOfWorkFalso(new CobrancaRepositorioFalso(Cobranca)), Pix, Notificador,
                NullLogger<BaixaCobrancaPix>.Instance);
        }
    }

    private class PixFalso : IPixService
    {
        private readonly string _status;
        public List<string> Consultas { get; } = new();
        public List<string> Canceladas { get; } = new();

        public PixFalso(string status) => _status = status;

        public Task<ConsultaCobrancaPix> ConsultarCobrancaAsync(string idExterno)
        {
            Consultas.Add(idExterno);
            return Task.FromResult(new ConsultaCobrancaPix(_status, _status == "RECEIVED" ? 405m : null, DateTime.Now, null));
        }

        public Task CancelarCobrancaAsync(string idExterno)
        {
            Canceladas.Add(idExterno);
            return Task.CompletedTask;
        }

        public List<NovaCobrancaPix> Criadas { get; } = new();
        public int ConsultasDeSubconta { get; private set; }

        public Task<PixGerado> CriarCobrancaAsync(NovaCobrancaPix cobranca)
        {
            Criadas.Add(cobranca);
            return Task.FromResult(new PixGerado("pay_novo", "000201...", "PENDING"));
        }

        public Task<SubcontaExistente?> ObterSubcontaAsync(string walletId) => throw new NotSupportedException();

        public Task<PagadorPix> PagadorPadraoDaSubcontaAsync(string walletId)
        {
            ConsultasDeSubconta++;
            return Task.FromResult(new PagadorPix("Clientes Sebrecon", "19183412000132", null));
        }

        public Task<SubcontaCriada> CriarSubcontaAsync(NovaSubconta subconta) => throw new NotSupportedException();
        public Task<SituacaoSubconta> ConsultarSituacaoSubcontaAsync(string apiKeySubconta) => throw new NotSupportedException();
        public Task TestarCredenciaisAsync() => Task.CompletedTask;
        public string LinkPagamento(string txid) => $"https://teste/pix/{txid}";
        public string UrlQrCode(string txid) => $"https://teste/pix/{txid}/qrcode.png";
    }

    private class NotificadorFalso : INotificadorCobranca
    {
        public List<(Guid, Guid, string)> Avisos { get; } = new();

        public Task NotificarCobrancaAtualizadaAsync(Guid empresaId, Guid cobrancaId, string status)
        {
            Avisos.Add((empresaId, cobrancaId, status));
            return Task.CompletedTask;
        }
    }

    private class CobrancaRepositorioFalso : ICobrancaClienteRepository
    {
        private readonly CobrancaCliente _cobranca;

        public CobrancaRepositorioFalso(CobrancaCliente cobranca) => _cobranca = cobranca;

        public Task<CobrancaCliente?> ObterPorTxid(string txid) =>
            Task.FromResult(txid == _cobranca.Txid ? _cobranca : null);

        public Task<IEnumerable<CobrancaCliente>> ObterPendentesComPix(DateTime expiradoDesde) =>
            Task.FromResult<IEnumerable<CobrancaCliente>>(new[] { _cobranca });

        public Task<int> BaixarPix(Guid id, DateTime dataPagamento, decimal valorPago, string? endToEndId)
        {
            if (id != _cobranca.Id || _cobranca.Status != StatusCobrancaCliente.Pendente) return Task.FromResult(0);
            _cobranca.Status = StatusCobrancaCliente.Paga;
            _cobranca.DataPagamento = dataPagamento;
            _cobranca.ValorPago = valorPago;
            return Task.FromResult(1);
        }

        public Task Incluir(CobrancaCliente cobranca) => throw new NotSupportedException();
        public Task<CobrancaCliente?> ObterPorId(Guid id, Guid? empresaIdSolicitante) => throw new NotSupportedException();
        public Task<IEnumerable<CobrancaCliente>> ObterPorEmpresa(Guid? empresaIdSolicitante, string? status, DateTime? dataInicio = null, DateTime? dataFim = null) => throw new NotSupportedException();
        public Task<int> MarcarPaga(Guid id, Guid? empresaIdSolicitante, DateTime dataPagamento) => throw new NotSupportedException();
    }

    private class UnitOfWorkFalso : IUnitOfWork
    {
        private readonly IDadosBancariosEmpresaRepository? _dadosBancarios;
        private readonly IEmpresaRepository? _empresa;

        public UnitOfWorkFalso(ICobrancaClienteRepository cobrancas, IDadosBancariosEmpresaRepository? dadosBancarios = null, IEmpresaRepository? empresa = null)
        {
            CobrancaCliente = cobrancas;
            _dadosBancarios = dadosBancarios;
            _empresa = empresa;
        }

        public ICobrancaClienteRepository CobrancaCliente { get; }

        public ICompaniesRepository CompaniesRepository => throw new NotSupportedException();
        public IFlowsRepository FlowsRepository => throw new NotSupportedException();
        public IFlowRepository Flow => throw new NotSupportedException();
        public IConversationsRepository ConversationsRepository => throw new NotSupportedException();
        public IEmpresaRepository Empresa => _empresa ?? throw new NotSupportedException();
        public IUsuarioRepository Usuario => throw new NotSupportedException();
        public IContatoRepository Contato => throw new NotSupportedException();
        public IPerfilRepository Perfil => throw new NotSupportedException();
        public IParametroRepository Parametro => throw new NotSupportedException();
        public INumeroRepository Numero => throw new NotSupportedException();
        public ITemplateRepository Template => throw new NotSupportedException();
        public IHistoricoDisparoRepository HistoricoDisparo => throw new NotSupportedException();
        public IConversationStateRepository ConversationState => throw new NotSupportedException();
        public ITagRepository Tag => throw new NotSupportedException();
        public ICampanhaRepository Campanha => throw new NotSupportedException();
        public IWebhookConfigRepository WebhookConfig => throw new NotSupportedException();
        public IProdutoRepository Produto => throw new NotSupportedException();
        public IPipelineRepository Pipeline => throw new NotSupportedException();
        public IMensagemRecebidaRepository MensagemRecebida => throw new NotSupportedException();
        public IRelatorioRepository Relatorio => throw new NotSupportedException();
        public IAssinaturaRepository Assinatura => throw new NotSupportedException();
        public IOrigemLeadRepository OrigemLead => throw new NotSupportedException();
        public IAgendamentoRepository Agendamento => throw new NotSupportedException();
        public IDadosBancariosEmpresaRepository DadosBancariosEmpresa => _dadosBancarios ?? throw new NotSupportedException();
        public void Commit() { }
        public void BeginTransaction() { }
        public void Rollback() { }
        public void Dispose() { }
    }
}
