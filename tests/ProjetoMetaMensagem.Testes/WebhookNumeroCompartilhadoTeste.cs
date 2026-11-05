using Microsoft.Extensions.Logging.Abstractions;
using ProjetoMetaMensagem.Dominio.Entidades;
using ProjetoMetaMensagem.Dominio.Helpers;
using ProjetoMetaMensagem.Dominio.Interfaces;
using ProjetoMetaMensagem.Dominio.Interfaces.Repositorios;
using ProjetoMetaMensagem.Dominio.Interfaces.Servicos;
using ProjetoMetaMensagem.Dominio.UseCases.Webhook.RecebeMensagemWebhook;
using System.Reflection;
using Xunit;

namespace ProjetoMetaMensagem.Testes;

// Sebrecon usa o numero da Contact Solution: a resposta do cliente tem que cair no chat e nos flows
// de quem falou com ele, e nao de quem e dona do numero.
public class WebhookNumeroCompartilhadoTeste
{
    private static readonly Guid ContactSolution = Guid.NewGuid();
    private static readonly Guid Sebrecon = Guid.NewGuid();
    private const string Telefone = "5511999990000";

    [Fact]
    public async Task Resposta_ao_template_da_Sebrecon_vai_para_o_chat_e_os_flows_da_Sebrecon()
    {
        var c = new Cenario { UltimoTemplate = Sebrecon };

        await c.Receber();

        Assert.Equal(Sebrecon, c.MensagemSalva!.EmpresaId);
        Assert.Equal(Sebrecon, c.EmpresaDoFlow);
        Assert.Equal(Sebrecon, c.EmpresaDoContatoBuscado);
    }

    [Fact]
    public async Task Botao_tocado_vale_mais_que_o_ultimo_template()
    {
        var c = new Cenario { UltimoTemplate = ContactSolution };
        c.Historicos["wamid.sebrecon"] = new HistoricoDisparo { EmpresaId = Sebrecon };

        await c.Receber(contexto: "wamid.sebrecon");

        Assert.Equal(Sebrecon, c.MensagemSalva!.EmpresaId);
    }

    [Fact]
    public async Task Contexto_de_outra_empresa_fora_do_numero_e_ignorado()
    {
        var c = new Cenario { UltimoTemplate = Sebrecon };
        c.Historicos["wamid.alheio"] = new HistoricoDisparo { EmpresaId = Guid.NewGuid() };

        await c.Receber(contexto: "wamid.alheio");

        Assert.Equal(Sebrecon, c.MensagemSalva!.EmpresaId);
    }

    [Fact]
    public async Task Sem_template_vai_para_quem_tem_o_telefone_cadastrado()
    {
        var c = new Cenario { EmpresasComContato = new() { Sebrecon } };

        await c.Receber();

        Assert.Equal(Sebrecon, c.MensagemSalva!.EmpresaId);
    }

    [Fact]
    public async Task Telefone_cadastrado_nas_duas_fica_com_a_dona()
    {
        var c = new Cenario { EmpresasComContato = new() { Sebrecon, ContactSolution } };

        await c.Receber();

        Assert.Equal(ContactSolution, c.MensagemSalva!.EmpresaId);
    }

    [Fact]
    public async Task Lead_novo_fica_com_a_dona_do_numero()
    {
        var c = new Cenario();

        await c.Receber();

        Assert.Equal(ContactSolution, c.MensagemSalva!.EmpresaId);
    }

    [Fact]
    public async Task Numero_de_uma_empresa_so_nao_faz_consulta_a_mais()
    {
        var c = new Cenario(apenasDona: true) { UltimoTemplate = Sebrecon };

        await c.Receber();

        Assert.Equal(ContactSolution, c.MensagemSalva!.EmpresaId);
        Assert.Equal(0, c.ConsultasDeUltimoTemplate);
    }

    [Fact]
    public async Task Flow_da_Sebrecon_nao_usa_o_numero_cadastrado_pela_Contact_Solution()
    {
        var numero = Guid.NewGuid();
        var c = new Cenario(numeroDaDona: numero) { UltimoTemplate = Sebrecon };

        await c.Receber();

        Assert.Equal(Sebrecon, c.EmpresaDoFlow);
        Assert.Null(c.NumeroDoFlow);
    }

    [Fact]
    public async Task Flow_da_dona_continua_usando_o_numero_dela()
    {
        var numero = Guid.NewGuid();
        var c = new Cenario(numeroDaDona: numero) { UltimoTemplate = ContactSolution };

        await c.Receber();

        Assert.Equal(numero, c.NumeroDoFlow);
    }

    [Fact]
    public async Task Reentrega_ja_salva_em_outra_empresa_do_numero_nao_processa_de_novo()
    {
        var c = new Cenario { UltimoTemplate = ContactSolution };
        c.JaProcessadaEm = Sebrecon;

        await c.Receber();

        Assert.Null(c.MensagemSalva);
        Assert.Null(c.EmpresaDoFlow);
    }

    [Fact]
    public async Task Status_de_entrega_vai_para_a_empresa_que_enviou()
    {
        var c = new Cenario();
        c.Historicos["wamid.sebrecon"] = new HistoricoDisparo { EmpresaId = Sebrecon };

        var resultado = await c.ReceberStatus("wamid.sebrecon");

        Assert.Equal(Sebrecon, resultado.Value.StatusAtualizados.Single().EmpresaId);
    }

    [Theory]
    [InlineData("5511999990000", "551199990000")] // Meta sem o 9, cadastro com
    [InlineData("551188887777", "5511988887777")] // cadastro sem o 9, Meta com
    [InlineData("5511999990000", "11999990000")] // cadastro sem o 55
    public void Variantes_cobrem_o_nono_digito_e_o_codigo_do_pais(string recebido, string cadastrado)
    {
        Assert.Contains(cadastrado, TelefoneHelper.VariantesParaBusca(recebido));
    }

    [Fact]
    public void Telefone_fixo_nao_ganha_nono_digito_e_numero_estrangeiro_fica_como_esta()
    {
        Assert.DoesNotContain("5511933334444", TelefoneHelper.VariantesParaBusca("551133334444"));
        Assert.Equal(new[] { "447911123456" }, TelefoneHelper.VariantesParaBusca("+44 7911 123456"));
    }

    private class Cenario
    {
        public Guid? UltimoTemplate { get; set; }
        public List<Guid> EmpresasComContato { get; set; } = new();
        public Dictionary<string, HistoricoDisparo> Historicos { get; } = new();
        public Guid? JaProcessadaEm { get; set; }
        public MensagemRecebida? MensagemSalva { get; private set; }
        public Guid? EmpresaDoFlow { get; private set; }
        public Guid? NumeroDoFlow { get; private set; }
        public Guid? EmpresaDoContatoBuscado { get; private set; }
        public int ConsultasDeUltimoTemplate { get; private set; }

        private readonly RecebeMensagemWebhookHandler _handler;

        public Cenario(bool apenasDona = false, Guid? numeroDaDona = null)
        {
            var usuario = Guid.NewGuid();
            var uow = Falso<IUnitOfWork>.Criar(new()
            {
                ["get_Numero"] = _ => Falso<INumeroRepository>.Criar(new()
                {
                    ["ObterPorInstanciaId"] = _ => Task.FromResult<Numero?>(numeroDaDona.HasValue ? new Numero { Id = numeroDaDona.Value, UsuarioId = usuario } : null)
                }),
                ["get_Usuario"] = _ => Falso<IUsuarioRepository>.Criar(new()
                {
                    ["ObterPorId"] = _ => Task.FromResult<Usuario?>(new Usuario { Id = usuario, EmpresaId = ContactSolution })
                }),
                ["get_Empresa"] = _ => Falso<IEmpresaRepository>.Criar(new()
                {
                    ["ObterTodasPorPhoneNumberId"] = _ => Task.FromResult(apenasDona ? new List<Guid> { ContactSolution } : new List<Guid> { ContactSolution, Sebrecon })
                }),
                ["get_HistoricoDisparo"] = _ => Falso<IHistoricoDisparoRepository>.Criar(new()
                {
                    ["ObterPorWamidMeta"] = a => Task.FromResult(Historicos.GetValueOrDefault((string)a[0]!)),
                    ["ObterEmpresaDoUltimoTemplate"] = _ => { ConsultasDeUltimoTemplate++; return Task.FromResult(UltimoTemplate); },
                    ["AtualizarStatusEntregaPorWamid"] = _ => Task.CompletedTask
                }),
                ["get_Contato"] = _ => Falso<IContatoRepository>.Criar(new()
                {
                    ["ObterEmpresasComTelefone"] = _ => Task.FromResult<IEnumerable<Guid>>(EmpresasComContato),
                    ["ObterPorTelefone"] = a =>
                    {
                        EmpresaDoContatoBuscado = (Guid)a[0]!;
                        return Task.FromResult<Contato?>(new Contato { Id = Guid.NewGuid(), EmpresaId = (Guid)a[0]! });
                    }
                }),
                ["get_MensagemRecebida"] = _ => Falso<IMensagemRecebidaRepository>.Criar(new()
                {
                    ["ExistePorWamid"] = a => Task.FromResult(JaProcessadaEm == (Guid)a[0]!),
                    ["Incluir"] = a => { MensagemSalva = (MensagemRecebida)a[0]!; return Task.CompletedTask; }
                }),
            });

            var flow = Falso<IFlowOrchestratorService>.Criar(new()
            {
                ["ProcessarMensagem"] = a =>
                {
                    EmpresaDoFlow = (Guid)a[0]!;
                    NumeroDoFlow = (Guid?)a[5];
                    return Task.FromResult(new FlowOrchestrationResult());
                }
            });
            var webhooks = Falso<IWebhookDispatcherService>.Criar(new() { ["Disparar"] = _ => Task.CompletedTask });

            _handler = new RecebeMensagemWebhookHandler(uow, flow, webhooks, NullLogger<RecebeMensagemWebhookHandler>.Instance);
        }

        public Task Receber(string? contexto = null) => _handler.Handle(Comando(new WebhookValueCommand
        {
            Metadata = new WebhookMetadataCommand { PhoneNumberId = "PHONE", DisplayPhoneNumber = "551140000000" },
            Messages = new()
            {
                new WebhookMessageCommand
                {
                    From = Telefone, Id = "wamid.recebida", Type = "text",
                    Text = new WebhookTextCommand { Body = "Oi" },
                    Context = contexto == null ? null : new WebhookContextCommand { Id = contexto }
                }
            }
        }));

        public Task<ProjetoMetaMensagem.Dominio.Common.Response<RecebeMensagemWebhookResult>> ReceberStatus(string wamid) => _handler.Handle(Comando(new WebhookValueCommand
        {
            Metadata = new WebhookMetadataCommand { PhoneNumberId = "PHONE" },
            Statuses = new() { new WebhookStatusCommand { Id = wamid, Status = "read" } }
        }));

        private static RecebeMensagemWebhookCommand Comando(WebhookValueCommand valor) => new()
        {
            Objeto = "whatsapp_business_account",
            Entry = new() { new WebhookEntryCommand { Changes = new() { new WebhookChangeCommand { Value = valor } } } }
        };
    }

    private class Falso<T> : DispatchProxy where T : class
    {
        private Dictionary<string, Func<object?[], object?>> _membros = new();

        public static T Criar(Dictionary<string, Func<object?[], object?>> membros)
        {
            var proxy = Create<T, Falso<T>>();
            ((Falso<T>)(object)proxy)._membros = membros;
            return proxy;
        }

        protected override object? Invoke(MethodInfo? metodo, object?[]? argumentos) =>
            _membros.TryGetValue(metodo!.Name, out var membro)
                ? membro(argumentos ?? Array.Empty<object?>())
                : throw new NotSupportedException($"{typeof(T).Name}.{metodo.Name} não definido no teste.");
    }
}
