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

        public Task<PixGerado> CriarCobrancaAsync(NovaCobrancaPix cobranca) => throw new NotSupportedException();
        public Task<SubcontaCriada> CriarSubcontaAsync(NovaSubconta subconta) => throw new NotSupportedException();
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
        public UnitOfWorkFalso(ICobrancaClienteRepository cobrancas) => CobrancaCliente = cobrancas;

        public ICobrancaClienteRepository CobrancaCliente { get; }

        public ICompaniesRepository CompaniesRepository => throw new NotSupportedException();
        public IFlowsRepository FlowsRepository => throw new NotSupportedException();
        public IFlowRepository Flow => throw new NotSupportedException();
        public IConversationsRepository ConversationsRepository => throw new NotSupportedException();
        public IEmpresaRepository Empresa => throw new NotSupportedException();
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
        public IDadosBancariosEmpresaRepository DadosBancariosEmpresa => throw new NotSupportedException();
        public void Commit() { }
        public void BeginTransaction() { }
        public void Rollback() { }
        public void Dispose() { }
    }
}
