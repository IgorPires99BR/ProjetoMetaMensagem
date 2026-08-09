using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using ProjetoMetaMensagem.Data;
using ProjetoMetaMensagem.Data.Repositorios;
using ProjetoMetaMensagem.Dominio.Common;
using ProjetoMetaMensagem.Dominio.Interfaces;
using ProjetoMetaMensagem.Dominio.Interfaces.Mediator;
using ProjetoMetaMensagem.Dominio.Interfaces.Repositorios;
using ProjetoMetaMensagem.Dominio.Interfaces.Servicos;
using ProjetoMetaMensagem.Dominio.UseCases.Auth.EsqueceuASenha;
using ProjetoMetaMensagem.Dominio.UseCases.Auth.Login;
using ProjetoMetaMensagem.Dominio.UseCases.Contato.AlteraContato;
using ProjetoMetaMensagem.Dominio.UseCases.Contato.CriaContato;
using ProjetoMetaMensagem.Dominio.UseCases.Contato.DeletaContato;
using ProjetoMetaMensagem.Dominio.UseCases.Contato.ObtemContato;
using ProjetoMetaMensagem.Dominio.UseCases.Empresa.AlteraEmpresa;
using ProjetoMetaMensagem.Dominio.UseCases.Empresa.CriaEmpresa;
using ProjetoMetaMensagem.Dominio.UseCases.Empresa.DeletaEmpresa;
using ProjetoMetaMensagem.Dominio.UseCases.Empresa.ObtemEmpresa;
using ProjetoMetaMensagem.Dominio.UseCases.Flows.CriaFlow;
using ProjetoMetaMensagem.Dominio.UseCases.Flows.ListaFlows;
using ProjetoMetaMensagem.Dominio.UseCases.Leads.ListaConversas;
using ProjetoMetaMensagem.Dominio.UseCases.Messages.CriarTemplateMeta;
using ProjetoMetaMensagem.Dominio.UseCases.Messages.EnviarMensagemMeta;
using ProjetoMetaMensagem.Dominio.UseCases.Messages.EnviarMensagemTemplateMeta;
using ProjetoMetaMensagem.Dominio.UseCases.Messages.EnviarMidiaMeta;
using ProjetoMetaMensagem.Dominio.UseCases.MensagemRecebida.ObtemMidia;
using ProjetoMetaMensagem.Dominio.UseCases.Numero.AlteraNumero;
using ProjetoMetaMensagem.Dominio.UseCases.Numero.CriaNumero;
using ProjetoMetaMensagem.Dominio.UseCases.Numero.DeletaNumero;
using ProjetoMetaMensagem.Dominio.UseCases.Numero.AtualizaNumeroMeta;
using ProjetoMetaMensagem.Dominio.UseCases.Numero.IniciaEmbeddedSignup;
using ProjetoMetaMensagem.Dominio.UseCases.Numero.AtivaCoexistencia;
using ProjetoMetaMensagem.Dominio.UseCases.Usuario.AlteraUsuario;
using ProjetoMetaMensagem.Dominio.UseCases.Usuario.CriaUsuario;
using ProjetoMetaMensagem.Dominio.UseCases.Usuario.DeletaUsuario;
using ProjetoMetaMensagem.Dominio.UseCases.Usuario.ObtemUsuario;
using ProjetoMetaMensagem.Servico.Configuration;
using ProjetoMetaMensagem.Servico.Email;
using MetaServiceImpl = ProjetoMetaMensagem.Servico.MetaService.MetaService;
using ProjetoMetaMensagem.Servico.Twilio;
using ProjetoMetaMensagem.Dominio.UseCases.Numero.ListarNumeros;
using ProjetoMetaMensagem.Dominio.UseCases.Template.CriaTemplate;
using ProjetoMetaMensagem.Dominio.UseCases.Template.DeletaTemplate;
using ProjetoMetaMensagem.Dominio.UseCases.Template.ListaTemplate;
using ProjetoMetaMensagem.Dominio.UseCases.Template.AtualizaTemplateMeta;
using ProjetoMetaMensagem.Dominio.UseCases.Template.ListaTemplateConexoes;
using ProjetoMetaMensagem.Dominio.UseCases.Template.CriaTemplateConexao;
using ProjetoMetaMensagem.Dominio.UseCases.Template.ExcluiTemplateConexao;
using ProjetoMetaMensagem.Dominio.UseCases.Template.UploadMidiaTemplate;
using ProjetoMetaMensagem.Dominio.UseCases.Messages.EnviarMensagemTemplateMetaLote;
using ProjetoMetaMensagem.Dominio.UseCases.Flows.AlteraFlow;
using ProjetoMetaMensagem.Dominio.UseCases.Empresa.AtualizaWabaId;
using ProjetoMetaMensagem.Dominio.UseCases.Contato.CriaContatoEmLote;
using ProjetoMetaMensagem.Servico.Flow;
using ProjetoMetaMensagem.Servico.Auth;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using ProjetoMetaMensagem.Dominio.UseCases.MensagemRecebida.ListaChatsAtivos;
using ProjetoMetaMensagem.Dominio.UseCases.MensagemRecebida.ListaMensagemRecebida;
using ProjetoMetaMensagem.Dominio.UseCases.MensagemRecebida.MarcarComoLida;
using ProjetoMetaMensagem.Dominio.UseCases.Relatorio.ListaRelatorioMensagens;
using ProjetoMetaMensagem.WebAPI.Hubs;
using ProjetoMetaMensagem.Dominio.UseCases.Webhook.RecebeMensagemWebhook;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowReactApp",
        policy =>
        {
            // AllowAnyOrigin() nao pode ser combinado com AllowCredentials() (a negociacao do
            // SignalR manda credenciais) — o navegador rejeita Access-Control-Allow-Origin: '*'
            // quando credentials mode e 'include'. Por isso a lista explicita + AllowCredentials.
            policy.WithOrigins("http://localhost:5173", "http://localhost:3000", "http://localhost:4200", "https://angularcontact.vercel.app", "https://contactsolution.com.br", "https://www.contactsolution.com.br")
                  .AllowAnyHeader()
                  .AllowAnyMethod()
                  .AllowCredentials();
        });
});

// Add services to the container.

builder.Services.AddControllers(options =>
{
    options.Filters.Add(new Microsoft.AspNetCore.Mvc.Authorization.AuthorizeFilter());
    options.Filters.Add<ProjetoMetaMensagem.WebAPI.Common.EmpresaAccessFilter>();
});
builder.Services.AddAuthorization();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddSignalR();

// JWT Authentication
var jwtSettings = builder.Configuration.GetSection("JwtSettings");
var secretKey = jwtSettings["SecretKey"] ?? "ChaveSuperSecretaMetaMensagem2026!@#";

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtSettings["Issuer"] ?? "ProjetoMetaMensagem",
        ValidAudience = jwtSettings["Audience"] ?? "ProjetoMetaMensagemApp",
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey)),
        ClockSkew = TimeSpan.Zero
    };
});

builder.Services.Configure<ApiWhatsappConnectionConfiguration>(builder.Configuration.GetSection("ApiWhatsappConnectionConfiguration"));

builder.Services.Configure<ApiWhatsappConnectionConfiguration>(builder.Configuration.GetSection("ApiWhatsappConnectionConfiguration"));

builder.Services.AddHttpClient<TwilioService>();

//mediator
builder.Services.AddScoped<IMediator, Mediator>();
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

//Banco
builder.Services.AddScoped<DbSession>();



//servicos
builder.Services.AddHttpClient<IWhatsappService, TwilioService>();
builder.Services.AddHttpClient<IMetaService, MetaServiceImpl>();
builder.Services.AddScoped<IEmailService, EmailService>();

//Configura��es

builder.Services.Configure<GmailConfiguration>(
    builder.Configuration.GetSection("GmailConfig"));

// Token Service
builder.Services.AddScoped<ITokenService, TokenService>();

//repositorios

builder.Services.AddScoped<ICompaniesRepository, CompaniesRepository>();
builder.Services.AddScoped<IFlowsRepository, FlowsRepository>();
builder.Services.AddScoped<IConversationsRepository, ConversationsRepository>();
builder.Services.AddScoped<IEmpresaRepository, EmpresaRepository>();
builder.Services.AddScoped<IUsuarioRepository, UsuarioRepository>();
builder.Services.AddScoped<IContatoRepository, ContatoRepository>();
builder.Services.AddScoped<ITemplateRepository, TemplateRepository>();
builder.Services.AddScoped<ITemplateConexaoRepository, TemplateConexaoRepository>();
builder.Services.AddScoped<INumeroRepository, NumeroRepository>();
builder.Services.AddScoped<IHistoricoDisparoRepository, HistoricoDisparoRepository>();
builder.Services.AddScoped<IFlowRepository, FlowRepository>();
builder.Services.AddScoped<ITagRepository, TagRepository>();
builder.Services.AddScoped<ICampanhaRepository, CampanhaRepository>();
builder.Services.AddScoped<IConversationStateRepository, ConversationStateRepository>();
builder.Services.AddScoped<IWebhookConfigRepository, WebhookConfigRepository>();
builder.Services.AddScoped<IProdutoRepository, ProdutoRepository>();
builder.Services.AddScoped<IPipelineRepository, PipelineRepository>();
builder.Services.AddScoped<IMensagemRecebidaRepository, MensagemRecebidaRepository>();
builder.Services.AddScoped<IRelatorioRepository, RelatorioRepository>();

// Flow Orchestrator
builder.Services.AddScoped<IFlowOrchestratorService, FlowOrchestratorService>();

//Inje��o de depend�ncia de Mensagens e disparos do Core
builder.Services.AddScoped<IRequestHandler<EnviarMensagemMetaCommand, Response<EnviarMensagemMetaResult>>, EnviarMensagemMetaHandler>();
builder.Services.AddScoped<IRequestHandler<CriarTemplateMetaCommand, Response<CriarTemplateMetaResult>>, CriarTemplateMetaHandler>();


//Estrutura de Login e esqueci minha senha montada
builder.Services.AddScoped<IRequestHandler<EsqueceuASenhaCommand, Response<EsqueceuASenhaResult>>, EsqueceuASenhaHandler>();
builder.Services.AddScoped<IRequestHandler<LoginCommand, Response<LoginResult>>, LoginHandler>();
builder.Services.AddScoped<IRequestHandler<ProjetoMetaMensagem.Dominio.UseCases.Dashboard.ObterMetricas.ObterMetricasCommand, Response<ProjetoMetaMensagem.Dominio.UseCases.Dashboard.ObterMetricas.ObterMetricasDashboardResult>>, ProjetoMetaMensagem.Dominio.UseCases.Dashboard.ObterMetricas.ObterMetricasHandler>();
builder.Services.AddScoped<IDashboardRepository, DashboardRepository>();

//Registros de Empresas
builder.Services.AddScoped<IRequestHandler<CriaEmpresaCommand, Response<CriaEmpresaResult>>, CriaEmpresaHandler>();
builder.Services.AddScoped<IRequestHandler<AlteraEmpresaCommand, Response<AlteraEmpresaResult>>, AlteraEmpresaHandler>();
builder.Services.AddScoped<IRequestHandler<DeletaEmpresaCommand, Response<DeletaEmpresaResult>>, DeletaEmpresaHandler>();
builder.Services.AddScoped<IRequestHandler< ObtemEmpresaCommand, Response<List<ObtemEmpresaResult>>>, ObtemEmpresaHandler>();
builder.Services.AddScoped<IRequestHandler<AtualizaWabaIdCommand, Response<AtualizaWabaIdResult>>, AtualizaWabaIdHandler>();


//Registros de Usuario
builder.Services.AddScoped<IRequestHandler<CriaUsuarioCommand, Response<CriaUsuarioResult>>, CriaUsuarioHandler>();
builder.Services.AddScoped<IRequestHandler<AlteraUsuarioCommand, Response<AlteraUsuarioResult>>, AlteraUsuarioHandler>();
builder.Services.AddScoped<IRequestHandler<DeletaUsuarioCommand, Response<DeletaUsuarioResult>>, DeletaUsuarioHandler>();
builder.Services.AddScoped<IRequestHandler<ObtemUsuarioCommand, Response<List<ObtemUsuarioResult>>>, ObtemUsuarioHandler>();

//Registros de Numero
builder.Services.AddScoped<IRequestHandler<CriaNumeroCommand, Response<CriaNumeroResult>>, CriaNumeroHandler>();
builder.Services.AddScoped<IRequestHandler<AlteraNumeroCommand, Response<AlteraNumeroResult>>, AlteraNumeroHandler>();
builder.Services.AddScoped<IRequestHandler<DeletaNumeroCommand, Response<DeletaNumeroResult>>, DeletaNumeroHandler>();
builder.Services.AddScoped<IRequestHandler<ListarNumerosCommand, Response<List<ListarNumerosResult>>>, ListarNumerosHandler>();
builder.Services.AddScoped<IRequestHandler<AtualizaNumeroMetaCommand, Response<List<AtualizaNumeroMetaResult>>>, AtualizaNumeroMetaHandler>();
builder.Services.AddScoped<IRequestHandler<IniciaEmbeddedSignupCommand, Response<IniciaEmbeddedSignupResult>>, IniciaEmbeddedSignupHandler>();
builder.Services.AddScoped<IRequestHandler<AtivaCoexistenciaCommand, Response<AtivaCoexistenciaResult>>, AtivaCoexistenciaHandler>();

//Registros de Template
builder.Services.AddScoped<IRequestHandler<CriaTemplateCommand, Response<CriaTemplateResult>>, CriaTemplateHandler>();
builder.Services.AddScoped<IRequestHandler<DeletaTemplateCommand, Response<DeletaTemplateResult>>, DeletaTemplateHandler>();
builder.Services.AddScoped<IRequestHandler<ListaTemplateCommand, Response<List<ListaTemplateResult>>>, ListaTemplateHandler>();
builder.Services.AddScoped<IRequestHandler<AtualizaTemplateMetaCommand, Response<AtualizaTemplateMetaResult>>, AtualizaTemplateMetaHandler>();
builder.Services.AddScoped<IRequestHandler<ListaTemplateConexoesCommand, Response<List<ListaTemplateConexoesResult>>>, ListaTemplateConexoesHandler>();
builder.Services.AddScoped<IRequestHandler<CriaTemplateConexaoCommand, Response<CriaTemplateConexaoResult>>, CriaTemplateConexaoHandler>();
builder.Services.AddScoped<IRequestHandler<ExcluiTemplateConexaoCommand, Response<ExcluiTemplateConexaoResult>>, ExcluiTemplateConexaoHandler>();
builder.Services.AddScoped<IRequestHandler<UploadMidiaTemplateCommand, Response<UploadMidiaTemplateResult>>, UploadMidiaTemplateHandler>();


//Registros de Contato
builder.Services.AddScoped<IRequestHandler<CriaContatoCommand, Response<CriaContatoResult>>, CriaContatoHandler>();
builder.Services.AddScoped<IRequestHandler<CriaContatoEmLoteCommand, Response<CriaContatoEmLoteResult>>, CriaContatoEmLoteHandler>();
builder.Services.AddScoped<IRequestHandler<AlteraContatoCommand, Response<AlteraContatoResult>>, AlteraContatoHandler>();
builder.Services.AddScoped<IRequestHandler<DeletaContatoCommand, Response<DeletaContatoResult>>, DeletaContatoHandler>();
builder.Services.AddScoped<IRequestHandler<ObtemContatoCommand, Response<List<ObtemContatoResult>>>, ObtemContatoHandler>();

//Registros de Mensagem
builder.Services.AddScoped<IRequestHandler<EnviarMensagemTemplateMetaCommand, Response<EnviarMensagemTemplateMetaResult>>, EnviarMensagemTemplateMetaHandler>();
builder.Services.AddScoped<IRequestHandler<EnviarMensagemTemplateMetaLoteCommand, Response<EnviarMensagemTemplateMetaLoteResult>>, EnviarMensagemTemplateMetaLoteHandler>();

//Registros de Mensagem
builder.Services.AddScoped<IRequestHandler<ListaChatsAtivosCommand, Response<ListaChatsAtivosResult>>, ListaChatsAtivosHandler>();
builder.Services.AddScoped<IRequestHandler<ListaMensagemRecebidaCommand, Response<ListaMensagemRecebidaResult>>, ListaMensagemRecebidaHandler>();
builder.Services.AddScoped<IRequestHandler<MarcarComoLidaCommand, Response<MarcarComoLidaResult>>, MarcarComoLidaHandler>();
builder.Services.AddScoped<IRequestHandler<ObtemMidiaCommand, Response<ObtemMidiaResult>>, ObtemMidiaHandler>();
builder.Services.AddScoped<IRequestHandler<EnviarMidiaMetaCommand, Response<EnviarMidiaMetaResult>>, EnviarMidiaMetaHandler>();
builder.Services.AddScoped<IRequestHandler<ListaRelatorioMensagensCommand, Response<ListaRelatorioMensagensResult>>, ListaRelatorioMensagensHandler>();



builder.Services.AddScoped<IRequestHandler<ListaFlowsCommand, Response<List<ListaFlowsResult>>>, ListaFlowsHandler>();
builder.Services.AddScoped<IRequestHandler<CriaFlowCommand, Response<CriaFlowResult>>, CriaFlowHandler>();
builder.Services.AddScoped<IRequestHandler<ProjetoMetaMensagem.Dominio.UseCases.Flows.DeletaFlow.DeletaFlowCommand, Response<ProjetoMetaMensagem.Dominio.UseCases.Flows.DeletaFlow.DeletaFlowResult>>, ProjetoMetaMensagem.Dominio.UseCases.Flows.DeletaFlow.DeletaFlowHandler>();
builder.Services.AddScoped<IRequestHandler<AlteraFlowCommand, Response<AlteraFlowResult>>, AlteraFlowHandler>();

//Registros de Convcrsations

builder.Services.AddScoped<IRequestHandler<ListaConversaPorIdCommand, Response<List<ListaConversaPorIdResult>>>, ListaConversaPorIdHandler>();
builder.Services.AddScoped<IRequestHandler<RecebeMensagemWebhookCommand, Response<RecebeMensagemWebhookResult>>, RecebeMensagemWebhookHandler>();

// Registros de Tag
// Estes handlers existiam e estavam expostos por controllers, mas nunca foram registrados aqui --
// como o Mediator resolve por DI, todo endpoint dessas areas respondia 500 em runtime
// ("No service for type IRequestHandler<...>"), sem nunca falhar no build.
builder.Services.AddScoped<IRequestHandler<ProjetoMetaMensagem.Dominio.UseCases.Tag.CriaTag.CriaTagCommand, Response<ProjetoMetaMensagem.Dominio.UseCases.Tag.CriaTag.CriaTagResult>>, ProjetoMetaMensagem.Dominio.UseCases.Tag.CriaTag.CriaTagHandler>();
builder.Services.AddScoped<IRequestHandler<ProjetoMetaMensagem.Dominio.UseCases.Tag.DeletaTag.DeletaTagCommand, Response<ProjetoMetaMensagem.Dominio.UseCases.Tag.DeletaTag.DeletaTagResult>>, ProjetoMetaMensagem.Dominio.UseCases.Tag.DeletaTag.DeletaTagHandler>();
builder.Services.AddScoped<IRequestHandler<ProjetoMetaMensagem.Dominio.UseCases.Tag.ListaTag.ListaTagCommand, Response<List<ProjetoMetaMensagem.Dominio.UseCases.Tag.ListaTag.ListaTagResult>>>, ProjetoMetaMensagem.Dominio.UseCases.Tag.ListaTag.ListaTagHandler>();
builder.Services.AddScoped<IRequestHandler<ProjetoMetaMensagem.Dominio.UseCases.Tag.AssociarTagsContato.AssociarTagsContatoCommand, Response<ProjetoMetaMensagem.Dominio.UseCases.Tag.AssociarTagsContato.AssociarTagsContatoResult>>, ProjetoMetaMensagem.Dominio.UseCases.Tag.AssociarTagsContato.AssociarTagsContatoHandler>();

// Registros de Produto
builder.Services.AddScoped<IRequestHandler<ProjetoMetaMensagem.Dominio.UseCases.Produto.CriaProduto.CriaProdutoCommand, Response<ProjetoMetaMensagem.Dominio.UseCases.Produto.CriaProduto.CriaProdutoResult>>, ProjetoMetaMensagem.Dominio.UseCases.Produto.CriaProduto.CriaProdutoHandler>();
builder.Services.AddScoped<IRequestHandler<ProjetoMetaMensagem.Dominio.UseCases.Produto.AlteraProduto.AlteraProdutoCommand, Response<ProjetoMetaMensagem.Dominio.UseCases.Produto.AlteraProduto.AlteraProdutoResult>>, ProjetoMetaMensagem.Dominio.UseCases.Produto.AlteraProduto.AlteraProdutoHandler>();
builder.Services.AddScoped<IRequestHandler<ProjetoMetaMensagem.Dominio.UseCases.Produto.DeletaProduto.DeletaProdutoCommand, Response<ProjetoMetaMensagem.Dominio.UseCases.Produto.DeletaProduto.DeletaProdutoResult>>, ProjetoMetaMensagem.Dominio.UseCases.Produto.DeletaProduto.DeletaProdutoHandler>();
builder.Services.AddScoped<IRequestHandler<ProjetoMetaMensagem.Dominio.UseCases.Produto.ListaProduto.ListaProdutoCommand, Response<List<ProjetoMetaMensagem.Dominio.UseCases.Produto.ListaProduto.ListaProdutoResult>>>, ProjetoMetaMensagem.Dominio.UseCases.Produto.ListaProduto.ListaProdutoHandler>();

// Registros de WebhookConfig
builder.Services.AddScoped<IRequestHandler<ProjetoMetaMensagem.Dominio.UseCases.Webhook.CriaWebhook.CriaWebhookCommand, Response<ProjetoMetaMensagem.Dominio.UseCases.Webhook.CriaWebhook.CriaWebhookResult>>, ProjetoMetaMensagem.Dominio.UseCases.Webhook.CriaWebhook.CriaWebhookHandler>();
builder.Services.AddScoped<IRequestHandler<ProjetoMetaMensagem.Dominio.UseCases.Webhook.DeletaWebhook.DeletaWebhookCommand, Response<ProjetoMetaMensagem.Dominio.UseCases.Webhook.DeletaWebhook.DeletaWebhookResult>>, ProjetoMetaMensagem.Dominio.UseCases.Webhook.DeletaWebhook.DeletaWebhookHandler>();
builder.Services.AddScoped<IRequestHandler<ProjetoMetaMensagem.Dominio.UseCases.Webhook.ListaWebhook.ListaWebhookCommand, Response<List<ProjetoMetaMensagem.Dominio.UseCases.Webhook.ListaWebhook.ListaWebhookResult>>>, ProjetoMetaMensagem.Dominio.UseCases.Webhook.ListaWebhook.ListaWebhookHandler>();

// Registros de Campanha
builder.Services.AddScoped<IRequestHandler<ProjetoMetaMensagem.Dominio.UseCases.Campanha.CriaCampanha.CriaCampanhaCommand, Response<ProjetoMetaMensagem.Dominio.UseCases.Campanha.CriaCampanha.CriaCampanhaResult>>, ProjetoMetaMensagem.Dominio.UseCases.Campanha.CriaCampanha.CriaCampanhaHandler>();
builder.Services.AddScoped<IRequestHandler<ProjetoMetaMensagem.Dominio.UseCases.Campanha.CancelaCampanha.CancelaCampanhaCommand, Response<ProjetoMetaMensagem.Dominio.UseCases.Campanha.CancelaCampanha.CancelaCampanhaResult>>, ProjetoMetaMensagem.Dominio.UseCases.Campanha.CancelaCampanha.CancelaCampanhaHandler>();
builder.Services.AddScoped<IRequestHandler<ProjetoMetaMensagem.Dominio.UseCases.Campanha.ListaCampanha.ListaCampanhaCommand, Response<List<ProjetoMetaMensagem.Dominio.UseCases.Campanha.ListaCampanha.ListaCampanhaResult>>>, ProjetoMetaMensagem.Dominio.UseCases.Campanha.ListaCampanha.ListaCampanhaHandler>();

// Registros de Pipeline (CRM)
builder.Services.AddScoped<IRequestHandler<ProjetoMetaMensagem.Dominio.UseCases.Pipeline.CriaPipeline.CriaPipelineCommand, Response<ProjetoMetaMensagem.Dominio.UseCases.Pipeline.CriaPipeline.CriaPipelineResult>>, ProjetoMetaMensagem.Dominio.UseCases.Pipeline.CriaPipeline.CriaPipelineHandler>();
builder.Services.AddScoped<IRequestHandler<ProjetoMetaMensagem.Dominio.UseCases.Pipeline.AlteraPipeline.AlteraPipelineCommand, Response<ProjetoMetaMensagem.Dominio.UseCases.Pipeline.AlteraPipeline.AlteraPipelineResult>>, ProjetoMetaMensagem.Dominio.UseCases.Pipeline.AlteraPipeline.AlteraPipelineHandler>();
builder.Services.AddScoped<IRequestHandler<ProjetoMetaMensagem.Dominio.UseCases.Pipeline.DeletaPipeline.DeletaPipelineCommand, Response<bool>>, ProjetoMetaMensagem.Dominio.UseCases.Pipeline.DeletaPipeline.DeletaPipelineHandler>();
builder.Services.AddScoped<IRequestHandler<ProjetoMetaMensagem.Dominio.UseCases.Pipeline.ListaPipeline.ListaPipelineCommand, Response<List<ProjetoMetaMensagem.Dominio.UseCases.Pipeline.ListaPipeline.ListaPipelineResult>>>, ProjetoMetaMensagem.Dominio.UseCases.Pipeline.ListaPipeline.ListaPipelineHandler>();
builder.Services.AddScoped<IRequestHandler<ProjetoMetaMensagem.Dominio.UseCases.Pipeline.ObtemPipelineComEtapas.ObtemPipelineComEtapasCommand, Response<ProjetoMetaMensagem.Dominio.UseCases.Pipeline.ObtemPipelineComEtapas.ObtemPipelineComEtapasResult>>, ProjetoMetaMensagem.Dominio.UseCases.Pipeline.ObtemPipelineComEtapas.ObtemPipelineComEtapasHandler>();

// Registros de Etapa do Pipeline
builder.Services.AddScoped<IRequestHandler<ProjetoMetaMensagem.Dominio.UseCases.PipelineEtapa.CriaEtapa.CriaEtapaCommand, Response<ProjetoMetaMensagem.Dominio.UseCases.PipelineEtapa.CriaEtapa.CriaEtapaResult>>, ProjetoMetaMensagem.Dominio.UseCases.PipelineEtapa.CriaEtapa.CriaEtapaHandler>();
builder.Services.AddScoped<IRequestHandler<ProjetoMetaMensagem.Dominio.UseCases.PipelineEtapa.AlteraEtapa.AlteraEtapaCommand, Response<ProjetoMetaMensagem.Dominio.UseCases.PipelineEtapa.AlteraEtapa.AlteraEtapaResult>>, ProjetoMetaMensagem.Dominio.UseCases.PipelineEtapa.AlteraEtapa.AlteraEtapaHandler>();
builder.Services.AddScoped<IRequestHandler<ProjetoMetaMensagem.Dominio.UseCases.PipelineEtapa.DeletaEtapa.DeletaEtapaCommand, Response<bool>>, ProjetoMetaMensagem.Dominio.UseCases.PipelineEtapa.DeletaEtapa.DeletaEtapaHandler>();
builder.Services.AddScoped<IRequestHandler<ProjetoMetaMensagem.Dominio.UseCases.PipelineEtapa.ListaEtapa.ListaEtapaCommand, Response<List<ProjetoMetaMensagem.Dominio.UseCases.PipelineEtapa.ListaEtapa.ListaEtapaResult>>>, ProjetoMetaMensagem.Dominio.UseCases.PipelineEtapa.ListaEtapa.ListaEtapaHandler>();

// Registros de Lead do Pipeline
builder.Services.AddScoped<IRequestHandler<ProjetoMetaMensagem.Dominio.UseCases.LeadPipeline.AdicionarLead.AdicionarLeadCommand, Response<ProjetoMetaMensagem.Dominio.UseCases.LeadPipeline.AdicionarLead.AdicionarLeadResult>>, ProjetoMetaMensagem.Dominio.UseCases.LeadPipeline.AdicionarLead.AdicionarLeadHandler>();
builder.Services.AddScoped<IRequestHandler<ProjetoMetaMensagem.Dominio.UseCases.LeadPipeline.MoverLead.MoverLeadCommand, Response<ProjetoMetaMensagem.Dominio.UseCases.LeadPipeline.MoverLead.MoverLeadResult>>, ProjetoMetaMensagem.Dominio.UseCases.LeadPipeline.MoverLead.MoverLeadHandler>();
builder.Services.AddScoped<IRequestHandler<ProjetoMetaMensagem.Dominio.UseCases.LeadPipeline.RemoverLead.RemoverLeadCommand, Response<bool>>, ProjetoMetaMensagem.Dominio.UseCases.LeadPipeline.RemoverLead.RemoverLeadHandler>();

var app = builder.Build();
 
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Projeto Meta Mensagem V1");
        // Se quiser que abra direto ao iniciar, deixe vazio. 
        // Se preferir acessar via /swagger, comente a linha abaixo.
        c.RoutePrefix = string.Empty;
    });



app.UseCors("AllowReactApp");

//app.UseHttpsRedirection();

app.UseMiddleware<ProjetoMetaMensagem.WebAPI.Common.ValidacaoAssinaturaMetaMiddleware>();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
// O front conecta em `${environment.apiUrl}/hubs/chat`, e apiUrl ja inclui "/api" --
// sem o prefixo aqui, o negotiate batia 404 (rota nao existia onde o front chamava).
app.MapHub<ChatHub>("/api/hubs/chat");

app.Run();
