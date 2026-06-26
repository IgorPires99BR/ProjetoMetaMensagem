using Microsoft.AspNetCore.Authentication.JwtBearer;
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
using ProjetoMetaMensagem.Dominio.UseCases.Numero.AlteraNumero;
using ProjetoMetaMensagem.Dominio.UseCases.Numero.CriaNumero;
using ProjetoMetaMensagem.Dominio.UseCases.Numero.DeletaNumero;
using ProjetoMetaMensagem.Dominio.UseCases.Numero.AtualizaNumeroMeta;
using ProjetoMetaMensagem.Dominio.UseCases.Usuario.AlteraUsuario;
using ProjetoMetaMensagem.Dominio.UseCases.Usuario.CriaUsuario;
using ProjetoMetaMensagem.Dominio.UseCases.Usuario.DeletaUsuario;
using ProjetoMetaMensagem.Dominio.UseCases.Usuario.ObtemUsuario;
using ProjetoMetaMensagem.Servico.Configuration;
using ProjetoMetaMensagem.Servico.Email;
using ProjetoMetaMensagem.Servico.Meta;
using ProjetoMetaMensagem.Servico.Twilio;
using ProjetoMetaMensagem.Servico.IA;
using ProjetoMetaMensagem.Dominio.UseCases.Numero.ListarNumeros;
using ProjetoMetaMensagem.Dominio.UseCases.Template.CriaTemplate;
using ProjetoMetaMensagem.Dominio.UseCases.Template.DeletaTemplate;
using ProjetoMetaMensagem.Dominio.UseCases.Template.ListaTemplate;
using ProjetoMetaMensagem.Dominio.UseCases.Template.AtualizaTemplateMeta;
using ProjetoMetaMensagem.Dominio.UseCases.Messages.EnviarMensagemTemplateMetaLote;
using ProjetoMetaMensagem.Dominio.UseCases.Flows.AlteraFlow;
using ProjetoMetaMensagem.Dominio.UseCases.Empresa.AtualizaWabaId;
using ProjetoMetaMensagem.Dominio.UseCases.Contato.CriaContatoEmLote;
using ProjetoMetaMensagem.Servico.Flow;
using ProjetoMetaMensagem.Servico.Auth;
using ProjetoMetaMensagem.Dominio.Interfaces.Servicos;
using System.Text;
using ProjetoMetaMensagem.Dominio.UseCases.Tag.CriaTag;
using ProjetoMetaMensagem.Dominio.UseCases.Tag.ListaTag;
using ProjetoMetaMensagem.Dominio.UseCases.Tag.DeletaTag;
using ProjetoMetaMensagem.Dominio.UseCases.Tag.AssociarTagsContato;
using ProjetoMetaMensagem.Dominio.UseCases.Campanha.CriaCampanha;
using ProjetoMetaMensagem.Dominio.UseCases.Campanha.ListaCampanha;
using ProjetoMetaMensagem.Dominio.UseCases.Campanha.CancelaCampanha;
using ProjetoMetaMensagem.Servico.Campanha;
using ProjetoMetaMensagem.Servico.Webhook;
using ProjetoMetaMensagem.Dominio.UseCases.Webhook.CriaWebhook;
using ProjetoMetaMensagem.Dominio.UseCases.Webhook.ListaWebhook;
using ProjetoMetaMensagem.Dominio.UseCases.Webhook.DeletaWebhook;
using ProjetoMetaMensagem.Dominio.UseCases.Produto.CriaProduto;
using ProjetoMetaMensagem.Dominio.UseCases.Produto.AlteraProduto;
using ProjetoMetaMensagem.Dominio.UseCases.Produto.DeletaProduto;
using ProjetoMetaMensagem.Dominio.UseCases.Produto.ListaProduto;
using ProjetoMetaMensagem.Dominio.UseCases.Dashboard.ObterMetricas;
using ProjetoMetaMensagem.Dominio.UseCases.Pipeline.Cria;
using ProjetoMetaMensagem.Dominio.UseCases.Pipeline.Lista;
using ProjetoMetaMensagem.Dominio.UseCases.Pipeline.Altera;
using ProjetoMetaMensagem.Dominio.UseCases.Pipeline.Deleta;
using ProjetoMetaMensagem.Dominio.UseCases.Pipeline.ObtemComEtapas;
using ProjetoMetaMensagem.Dominio.UseCases.PipelineEtapa.Cria;
using ProjetoMetaMensagem.Dominio.UseCases.PipelineEtapa.Lista;
using ProjetoMetaMensagem.Dominio.UseCases.PipelineEtapa.Altera;
using ProjetoMetaMensagem.Dominio.UseCases.PipelineEtapa.Deleta;
using ProjetoMetaMensagem.Dominio.UseCases.LeadPipeline.Mover;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowReactApp",
        policy =>
        {
            policy.WithOrigins("http://localhost:5173", "http://localhost:3000", "http://localhost:4200", "https://angularcontact.vercel.app")
                  .AllowAnyHeader()
                  .AllowAnyMethod()
                  .AllowAnyOrigin();
        });
});

// Add services to the container.

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

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

builder.Services.Configure<GeminiConfiguration>(builder.Configuration.GetSection("GeminiConfiguration"));

builder.Services.AddHttpClient<TwilioService>();
builder.Services.AddHttpClient<IGeminiService, GeminiService>();

//mediator
builder.Services.AddScoped<IMediator, Mediator>();
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

//Banco
builder.Services.AddScoped<DbSession>();



//servicos
builder.Services.AddHttpClient<IWhatsappService, TwilioService>();
builder.Services.AddHttpClient<IMetaService, MetaService>();
builder.Services.AddHttpClient<IEmailService, EmailService>();

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
builder.Services.AddScoped<INumeroRepository, NumeroRepository>();
builder.Services.AddScoped<IHistoricoDisparoRepository, HistoricoDisparoRepository>();
builder.Services.AddScoped<IFlowRepository, FlowRepository>();
builder.Services.AddScoped<IConversationStateRepository, ConversationStateRepository>();

// Webhook
builder.Services.AddScoped<IWebhookConfigRepository, WebhookConfigRepository>();
builder.Services.AddScoped<WebhookDispatcherService>();
builder.Services.AddScoped<IRequestHandler<CriaWebhookCommand, Response<CriaWebhookResult>>, CriaWebhookHandler>();
builder.Services.AddScoped<IRequestHandler<ListaWebhookCommand, Response<List<ListaWebhookResult>>>, ListaWebhookHandler>();
builder.Services.AddScoped<IRequestHandler<DeletaWebhookCommand, Response<DeletaWebhookResult>>, DeletaWebhookHandler>();

// Flow Orchestrator
builder.Services.AddScoped<IFlowOrchestratorService, FlowOrchestratorService>();

//Inje��o de depend�ncia de Mensagens e disparos do Core
builder.Services.AddScoped<IRequestHandler<EnviarMensagemMetaCommand, Response<EnviarMensagemMetaResult>>, EnviarMensagemMetaHandler>();
builder.Services.AddScoped<IRequestHandler<CriarTemplateMetaCommand, Response<CriarTemplateMetaResult>>, CriarTemplateMetaHandler>();


//Estrutura de Login e esqueci minha senha montada
builder.Services.AddScoped<IRequestHandler<EsqueceuASenhaCommand, Response<EsqueceuASenhaResult>>, EsqueceuASenhaHandler>();
builder.Services.AddScoped<IRequestHandler<LoginCommand, Response<LoginResult>>, LoginHandler>();

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

//Registros de Template
builder.Services.AddScoped<IRequestHandler<CriaTemplateCommand, Response<CriaTemplateResult>>, CriaTemplateHandler>();
builder.Services.AddScoped<IRequestHandler<DeletaTemplateCommand, Response<DeletaTemplateResult>>, DeletaTemplateHandler>();
builder.Services.AddScoped<IRequestHandler<ListaTemplateCommand, Response<List<ListaTemplateResult>>>, ListaTemplateHandler>();
builder.Services.AddScoped<IRequestHandler<AtualizaTemplateMetaCommand, Response<AtualizaTemplateMetaResult>>, AtualizaTemplateMetaHandler>();


//Registros de Contato
builder.Services.AddScoped<IRequestHandler<CriaContatoCommand, Response<CriaContatoResult>>, CriaContatoHandler>();
builder.Services.AddScoped<IRequestHandler<CriaContatoEmLoteCommand, Response<CriaContatoEmLoteResult>>, CriaContatoEmLoteHandler>();
builder.Services.AddScoped<IRequestHandler<AlteraContatoCommand, Response<AlteraContatoResult>>, AlteraContatoHandler>();
builder.Services.AddScoped<IRequestHandler<DeletaContatoCommand, Response<DeletaContatoResult>>, DeletaContatoHandler>();
builder.Services.AddScoped<IRequestHandler<ObtemContatoCommand, Response<List<ObtemContatoResult>>>, ObtemContatoHandler>();

//Registros de Mensagem
builder.Services.AddScoped<IRequestHandler<EnviarMensagemTemplateMetaCommand, Response<EnviarMensagemTemplateMetaResult>>, EnviarMensagemTemplateMetaHandler>();
builder.Services.AddScoped<IRequestHandler<EnviarMensagemTemplateMetaLoteCommand, Response<EnviarMensagemTemplateMetaLoteResult>>, EnviarMensagemTemplateMetaLoteHandler>();


builder.Services.AddScoped<IRequestHandler<ListaFlowsCommand, Response<List<ListaFlowsResult>>>, ListaFlowsHandler>();
builder.Services.AddScoped<IRequestHandler<CriaFlowCommand, Response<CriaFlowResult>>, CriaFlowHandler>();
builder.Services.AddScoped<IRequestHandler<AlteraFlowCommand, Response<AlteraFlowResult>>, AlteraFlowHandler>();

//Registros de Convcrsations

builder.Services.AddScoped<IRequestHandler<ListaConversaPorIdCommand, Response<List<ListaConversaPorIdResult>>>, ListaConversaPorIdHandler>();

// Tags
builder.Services.AddScoped<ITagRepository, TagRepository>();
builder.Services.AddScoped<IRequestHandler<CriaTagCommand, Response<CriaTagResult>>, CriaTagHandler>();
builder.Services.AddScoped<IRequestHandler<ListaTagCommand, Response<List<ListaTagResult>>>, ListaTagHandler>();
builder.Services.AddScoped<IRequestHandler<DeletaTagCommand, Response<DeletaTagResult>>, DeletaTagHandler>();
builder.Services.AddScoped<IRequestHandler<AssociarTagsContatoCommand, Response<AssociarTagsContatoResult>>, AssociarTagsContatoHandler>();

// Campanha
builder.Services.AddScoped<ICampanhaRepository, CampanhaRepository>();
builder.Services.AddScoped<IRequestHandler<CriaCampanhaCommand, Response<CriaCampanhaResult>>, CriaCampanhaHandler>();
builder.Services.AddScoped<IRequestHandler<ListaCampanhaCommand, Response<List<ListaCampanhaResult>>>, ListaCampanhaHandler>();
builder.Services.AddScoped<IRequestHandler<CancelaCampanhaCommand, Response<CancelaCampanhaResult>>, CancelaCampanhaHandler>();
builder.Services.AddHostedService<CampanhaWorker>();

	// Produto
	builder.Services.AddScoped<IProdutoRepository, ProdutoRepository>();
	builder.Services.AddScoped<IRequestHandler<CriaProdutoCommand, Response<CriaProdutoResult>>, CriaProdutoHandler>();
	builder.Services.AddScoped<IRequestHandler<AlteraProdutoCommand, Response<AlteraProdutoResult>>, AlteraProdutoHandler>();
	builder.Services.AddScoped<IRequestHandler<DeletaProdutoCommand, Response<DeletaProdutoResult>>, DeletaProdutoHandler>();
	builder.Services.AddScoped<IRequestHandler<ListaProdutoCommand, Response<List<ListaProdutoResult>>>, ListaProdutoHandler>();

	// Webhook
	builder.Services.AddScoped<IWebhookConfigRepository, WebhookConfigRepository>();
	// Dashboard
	builder.Services.AddScoped<IDashboardRepository, DashboardRepository>();
	builder.Services.AddScoped<IRequestHandler<ObterMetricasCommand, Response<ObterMetricasDashboardResult>>, ObterMetricasHandler>();

	// Pipeline
	builder.Services.AddScoped<IPipelineRepository, PipelineRepository>();
  builder.Services.AddScoped<IMensagemRecebidaRepository, MensagemRecebidaRepository>();
	builder.Services.AddScoped<IRequestHandler<CriaPipelineCommand, Response<CriaPipelineResult>>, CriaPipelineHandler>();
	builder.Services.AddScoped<IRequestHandler<ListaPipelineCommand, Response<List<ListaPipelineResult>>>, ListaPipelineHandler>();
	builder.Services.AddScoped<IRequestHandler<AlteraPipelineCommand, Response<AlteraPipelineResult>>, AlteraPipelineHandler>();
	builder.Services.AddScoped<IRequestHandler<DeletaPipelineCommand, Response<bool>>, DeletaPipelineHandler>();
	builder.Services.AddScoped<IRequestHandler<ObtemPipelineComEtapasCommand, Response<ObtemPipelineComEtapasResult>>, ObtemPipelineComEtapasHandler>();
	builder.Services.AddScoped<IRequestHandler<CriaEtapaCommand, Response<CriaEtapaResult>>, CriaEtapaHandler>();
	builder.Services.AddScoped<IRequestHandler<ListaEtapaCommand, Response<List<ListaEtapaResult>>>, ListaEtapaHandler>();
	builder.Services.AddScoped<IRequestHandler<AlteraEtapaCommand, Response<AlteraEtapaResult>>, AlteraEtapaHandler>();
	builder.Services.AddScoped<IRequestHandler<DeletaEtapaCommand, Response<bool>>, DeletaEtapaHandler>();
	builder.Services.AddScoped<IRequestHandler<MoverLeadCommand, Response<MoverLeadResult>>, MoverLeadHandler>();
	builder.Services.AddScoped<IRequestHandler<AdicionarLeadCommand, Response<MoverLeadResult>>, AdicionarLeadHandler>();
	builder.Services.AddScoped<IRequestHandler<RemoverLeadCommand, Response<bool>>, RemoverLeadHandler>();

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

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
