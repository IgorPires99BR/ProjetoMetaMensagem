using Microsoft.Extensions.Logging.Abstractions;
using ProjetoMetaMensagem.Dominio.Entidades;
using ProjetoMetaMensagem.Dominio.Interfaces;
using ProjetoMetaMensagem.Dominio.Interfaces.Repositorios;
using ProjetoMetaMensagem.Dominio.Interfaces.Servicos;
using ProjetoMetaMensagem.Dominio.Common;
using ProjetoMetaMensagem.Dominio.Servicos;
using ProjetoMetaMensagem.Dominio.UseCases.CobrancaCliente.SimulaPagamentoSandbox;
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
    public async Task Recebimento_na_plataforma_gera_Pix_sem_split_mesmo_sem_subconta_e_comissao()
    {
        var (pix, preparada) = await PrepararPara(cpfCnpj: "52998224725", recebimentoNaPlataforma: true, semSubconta: true);

        Assert.True(preparada.TemPix, preparada.Erro);
        var criada = pix.Criadas.Single();
        Assert.Null(criada.WalletIdRepasse);
        Assert.Null(criada.ValorRepasse);
        Assert.True(preparada.Cobranca.RecebidaNaPlataforma);
        Assert.Null(preparada.Cobranca.ValorComissao);
        Assert.Null(preparada.Cobranca.ValorRepasse);
    }

    [Fact]
    public async Task Recebimento_na_plataforma_ignora_a_subconta_vinculada()
    {
        var (pix, preparada) = await PrepararPara(cpfCnpj: null, recebimentoNaPlataforma: true);

        Assert.True(preparada.TemPix, preparada.Erro);
        Assert.Null(pix.Criadas.Single().WalletIdRepasse);
        // Cliente padrao sai do cadastro da empresa, sem consultar a subconta.
        Assert.Equal(new PagadorPix("Clientes Sebrecon", "19183412000132", null), pix.Criadas.Single().Pagador);
        Assert.Equal(0, pix.ConsultasDeSubconta);
    }

    [Fact]
    public async Task Com_subconta_o_Pix_continua_com_split()
    {
        var (_, preparada) = await PrepararPara(cpfCnpj: "52998224725");

        Assert.False(preparada.Cobranca.RecebidaNaPlataforma);
        Assert.Equal(3.50m, preparada.Cobranca.ValorComissao);
    }

    [Fact]
    public async Task Contato_sem_dados_de_cobranca_nao_gera_Pix()
    {
        var (pix, preparada) = await PrepararPara(cpfCnpj: "52998224725", semJuros: true);

        Assert.False(preparada.TemPix);
        Assert.Contains("juros", preparada.Erro);
        Assert.Empty(pix.Criadas);
    }

    [Fact]
    public async Task Simular_pagamento_no_sandbox_paga_no_Asaas_e_ja_da_baixa()
    {
        var cenario = new Cenario(statusNoAsaas: "PENDING");

        var resposta = await cenario.SimularPagamento();

        Assert.Equal(new[] { "pay_123" }, cenario.Pix.PagamentosSimulados);
        Assert.Equal(StatusCobrancaCliente.Paga, cenario.Cobranca.Status);
        Assert.Equal(StatusCobrancaCliente.Paga, resposta.Value.Status);
        Assert.Single(cenario.Notificador.Avisos);
    }

    [Fact]
    public async Task Simular_pagamento_e_recusado_com_o_Asaas_em_producao()
    {
        var cenario = new Cenario(statusNoAsaas: "PENDING");
        cenario.Pix.Sandbox = false;

        var resposta = await cenario.SimularPagamento();

        Assert.True(resposta.HasValidations);
        Assert.Empty(cenario.Pix.PagamentosSimulados);
        Assert.Equal(StatusCobrancaCliente.Pendente, cenario.Cobranca.Status);
    }

    [Fact]
    public async Task Simular_pagamento_de_cobranca_ja_paga_nao_chama_o_Asaas()
    {
        var cenario = new Cenario(statusNoAsaas: "RECEIVED");
        cenario.Cobranca.Status = StatusCobrancaCliente.Paga;

        var resposta = await cenario.SimularPagamento();

        Assert.True(resposta.HasValidations);
        Assert.Empty(cenario.Pix.PagamentosSimulados);
    }

    // --- Mercado Pago como provedor ativo (Asaas de reserva) ---

    [Fact]
    public async Task Mercado_Pago_gera_a_order_sem_split_e_sem_exigir_subconta_nem_comissao()
    {
        var mp = new MercadoPagoFalso("action_required");

        var (asaas, preparada) = await PrepararPara(cpfCnpj: null, semSubconta: true, ativo: mp, email: "maria@exemplo.com");

        Assert.True(preparada.TemPix, preparada.Erro);
        var criada = Assert.Single(mp.Criadas);
        Assert.Null(criada.WalletIdRepasse);
        Assert.Equal(405m, criada.Valor);
        // Sem documento do contato: nada de cliente padrao da empresa, so o e-mail dele.
        Assert.Equal("", criada.Pagador.CpfCnpj);
        Assert.Equal("maria@exemplo.com", criada.Pagador.Email);
        Assert.Empty(asaas.Criadas);
        Assert.Equal(0, asaas.ConsultasDeSubconta);

        Assert.Equal(ProvedorCobranca.MercadoPago, preparada.Cobranca.Provedor);
        Assert.Equal("ORD_NOVA", preparada.Cobranca.PixIdExterno);
        Assert.Equal("https://www.mercadopago.com.br/payments/1/ticket", preparada.Cobranca.LinkPagamentoExterno);
        Assert.True(preparada.Cobranca.RecebidaNaPlataforma);
        Assert.Null(preparada.Cobranca.ValorRepasse);
        // O link enviado no WhatsApp continua sendo a nossa pagina (botao do template aprovado).
        Assert.Equal($"https://teste/pix/{preparada.Cobranca.Txid}", preparada.LinkPagamento);
    }

    [Fact]
    public async Task Webhook_do_Mercado_Pago_da_baixa_reconsultando_a_order()
    {
        var mp = new MercadoPagoFalso("processed");
        var cenario = new Cenario(statusNoAsaas: "PENDING", ativo: mp, provedorDaCobranca: ProvedorCobranca.MercadoPago);

        await cenario.Baixa.ProcessarWebhookAsync(new[] { cenario.Cobranca.Txid! });

        Assert.Equal(new[] { "pay_123" }, mp.Consultas);
        Assert.Empty(cenario.Pix.Consultas);
        Assert.Equal(StatusCobrancaCliente.Paga, cenario.Cobranca.Status);
        Assert.Equal(405m, cenario.Cobranca.ValorPago);
    }

    [Fact]
    public async Task Order_ainda_aguardando_Pix_nao_da_baixa()
    {
        var mp = new MercadoPagoFalso("action_required");
        var cenario = new Cenario(statusNoAsaas: "PENDING", ativo: mp, provedorDaCobranca: ProvedorCobranca.MercadoPago);

        await cenario.Baixa.ProcessarWebhookAsync(new[] { cenario.Cobranca.Txid! });

        Assert.Equal(StatusCobrancaCliente.Pendente, cenario.Cobranca.Status);
        Assert.Empty(cenario.Notificador.Avisos);
    }

    [Fact]
    public async Task Cobranca_antiga_do_Asaas_continua_sendo_baixada_no_Asaas_com_Mercado_Pago_ativo()
    {
        var mp = new MercadoPagoFalso("processed");
        var cenario = new Cenario(statusNoAsaas: "RECEIVED", ativo: mp, provedorDaCobranca: null);

        await cenario.Baixa.ProcessarWebhookAsync(new[] { cenario.Cobranca.Txid! });

        Assert.Equal(new[] { "pay_123" }, cenario.Pix.Consultas);
        Assert.Empty(mp.Consultas);
        Assert.Equal(StatusCobrancaCliente.Paga, cenario.Cobranca.Status);
    }

    [Fact]
    public async Task Order_expirada_sem_pagamento_e_cancelada_no_Mercado_Pago()
    {
        var mp = new MercadoPagoFalso("action_required");
        var cenario = new Cenario(statusNoAsaas: "PENDING", ativo: mp, provedorDaCobranca: ProvedorCobranca.MercadoPago);
        cenario.Cobranca.PixExpiraEm = HorarioBrasilia.Agora().AddHours(-1);

        await cenario.Baixa.ConsultarPendentesAsync();

        Assert.Equal(new[] { "pay_123" }, mp.Canceladas);
        Assert.Empty(cenario.Pix.Canceladas);
    }

    [Fact]
    public async Task Simular_pagamento_nao_vale_para_order_do_Mercado_Pago()
    {
        var mp = new MercadoPagoFalso("action_required");
        var cenario = new Cenario(statusNoAsaas: "PENDING", ativo: mp, provedorDaCobranca: ProvedorCobranca.MercadoPago);

        var resposta = await cenario.SimularPagamento();

        Assert.True(resposta.HasValidations);
        Assert.Empty(cenario.Pix.PagamentosSimulados);
    }

    private static async Task<(PixFalso, CobrancaPreparada)> PrepararPara(string? cpfCnpj, bool carteiraExterna = false,
        string cnpjEmpresa = "19.183.412/0001-32", bool semJuros = false, bool recebimentoNaPlataforma = false, bool semSubconta = false,
        IProvedorCobranca? ativo = null, string? email = null)
    {
        var empresaId = Guid.NewGuid();
        var contato = new Contato
        {
            Id = Guid.NewGuid(), EmpresaId = empresaId, Telefone = "5511999990000",
            NomeCliente = "Maria Souza", DiaVencimento = 20, ValorFatura = 405m, CpfCnpj = cpfCnpj, Email = email,
            TaxaJuros = semJuros ? null : 2m, TaxaJurosMensal = semJuros ? null : 1m
        };
        var dados = new DadosBancariosEmpresa
        {
            EmpresaId = empresaId, CobrancaPixAtiva = true, RecebimentoNaPlataforma = recebimentoNaPlataforma,
            AsaasWalletId = semSubconta ? null : "wal_sebrecon",
            AsaasContaId = carteiraExterna || semSubconta ? null : "acc_sebrecon",
            ComissaoTipo = semSubconta ? null : DadosBancariosEmpresa.ComissaoFixa, ComissaoValor = semSubconta ? null : 3.50m
        };
        var pix = new PixFalso("PENDING");
        var uow = new UnitOfWorkFalso(new CobrancaRepositorioFalso(new CobrancaCliente()), new DadosBancariosFalso(dados),
            new EmpresaFalsa(new Empresa { Id = empresaId, Nome = "Sebrecon", Cnpj = cnpjEmpresa }));

        var preparadas = await new PreparadorCobrancaPix(uow, pix, new ProvedoresFalsos(ativo ?? pix, pix), NullLogger<PreparadorCobrancaPix>.Instance)
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

        public Cenario(string statusNoAsaas, IProvedorCobranca? ativo = null, string? provedorDaCobranca = null)
        {
            Cobranca = new CobrancaCliente
            {
                EmpresaId = Guid.NewGuid(),
                Valor = 405m,
                ValorCobrado = 405m,
                PixIdExterno = "pay_123",
                PixExpiraEm = HorarioBrasilia.Agora().AddHours(5),
                Provedor = provedorDaCobranca
            };
            Cobranca.Txid = CobrancaCliente.TxidDe(Cobranca.Id);

            Pix = new PixFalso(statusNoAsaas);
            _unitOfWork = new UnitOfWorkFalso(new CobrancaRepositorioFalso(Cobranca));
            Baixa = new BaixaCobrancaPix(_unitOfWork, new ProvedoresFalsos(ativo ?? Pix, Pix), Notificador, NullLogger<BaixaCobrancaPix>.Instance);
        }

        private readonly UnitOfWorkFalso _unitOfWork;

        public Task<Response<SimulaPagamentoSandboxResult>> SimularPagamento() =>
            new SimulaPagamentoSandboxHandler(_unitOfWork, Pix, Baixa, NullLogger<SimulaPagamentoSandboxHandler>.Instance)
                .Handle(new SimulaPagamentoSandboxCommand { CobrancaClienteId = Cobranca.Id });
    }

    private class PixFalso : IPixService, IProvedorCobranca
    {
        private string _status;
        public List<string> Consultas { get; } = new();
        public List<string> Canceladas { get; } = new();

        public PixFalso(string status) => _status = status;

        public string Nome => ProvedorCobranca.Asaas;
        public bool SuportaSplit => true;

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

        public bool Sandbox { get; set; } = true;
        public List<string> PagamentosSimulados { get; } = new();

        // Como no sandbox real: depois do confirm o GET /payments ja devolve RECEIVED.
        public Task<string> ConfirmarPagamentoSandboxAsync(string idExterno)
        {
            PagamentosSimulados.Add(idExterno);
            _status = "RECEIVED";
            return Task.FromResult(_status);
        }
        public string LinkPagamento(string txid) => $"https://teste/pix/{txid}";
        public string UrlQrCode(string txid) => $"https://teste/pix/{txid}/qrcode.png";
    }

    private class ProvedoresFalsos : IProvedoresCobranca
    {
        private readonly IProvedorCobranca[] _todos;

        public ProvedoresFalsos(IProvedorCobranca ativo, params IProvedorCobranca[] outros)
        {
            Ativo = ativo;
            _todos = outros.Prepend(ativo).ToArray();
        }

        public IProvedorCobranca Ativo { get; }

        public IProvedorCobranca Do(string? nome) =>
            _todos.First(p => p.Nome == (nome ?? ProvedorCobranca.Asaas));
    }

    // Status da order: "processed" = paga (accredited), "action_required" = aguardando o Pix.
    private class MercadoPagoFalso : IProvedorCobranca
    {
        private readonly string _status;
        public List<NovaCobrancaPix> Criadas { get; } = new();
        public List<string> Consultas { get; } = new();
        public List<string> Canceladas { get; } = new();

        public MercadoPagoFalso(string status) => _status = status;

        public string Nome => ProvedorCobranca.MercadoPago;
        public bool SuportaSplit => false;

        public Task<PixGerado> CriarCobrancaAsync(NovaCobrancaPix cobranca)
        {
            Criadas.Add(cobranca);
            return Task.FromResult(new PixGerado("ORD_NOVA", "000201...mp", "action_required", "https://www.mercadopago.com.br/payments/1/ticket"));
        }

        public Task<ConsultaCobrancaPix> ConsultarCobrancaAsync(string idExterno)
        {
            Consultas.Add(idExterno);
            var paga = _status == "processed";
            return Task.FromResult(new ConsultaCobrancaPix(_status, paga ? 405m : null, paga ? DateTime.Now : null, null,
                PagaNoProvedor: paga, AguardandoPagamento: _status == "action_required"));
        }

        public Task CancelarCobrancaAsync(string idExterno)
        {
            Canceladas.Add(idExterno);
            return Task.CompletedTask;
        }

        public Task TestarCredenciaisAsync() => Task.CompletedTask;
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
        public Task<CobrancaCliente?> ObterPorId(Guid id, Guid? empresaIdSolicitante) => Task.FromResult<CobrancaCliente?>(id == _cobranca.Id ? _cobranca : null);
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
