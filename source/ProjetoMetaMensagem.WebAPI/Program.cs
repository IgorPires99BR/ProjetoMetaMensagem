using ProjetoMetaMensagem.WebAPI.Common;
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
using ProjetoMetaMensagem.Dominio.UseCases.Template.AtualizaTemplate;
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
using ProjetoMetaMensagem.Dominio.UseCases.MensagemRecebida.AssumirConversa;
using ProjetoMetaMensagem.Dominio.UseCases.MensagemRecebida.DevolverAoBot;
using ProjetoMetaMensagem.Dominio.UseCases.Cobranca.ListaAssinaturas;
using ProjetoMetaMensagem.Dominio.UseCases.Cobranca.ProcessaEventoCakto;
using ProjetoMetaMensagem.Dominio.UseCases.Relatorio.ListaRelatorioMensagens;
using ProjetoMetaMensagem.Dominio.UseCases.Relatorio.ObtemRelatorioFinanceiro;
using ProjetoMetaMensagem.Dominio.UseCases.Relatorio.ObtemRelatorioEngajamento;
using ProjetoMetaMensagem.Dominio.UseCases.Relatorio.ObtemPrecoCategoria;
using ProjetoMetaMensagem.Dominio.UseCases.Relatorio.AtualizaPrecoCategoria;
using ProjetoMetaMensagem.Dominio.UseCases.IA.SugerirTexto;
using ProjetoMetaMensagem.WebAPI.Hubs;
using ProjetoMetaMensagem.Dominio.UseCases.Webhook.RecebeMensagemWebhook;
using ProjetoMetaMensagem.Servico.Campanha;
using ProjetoMetaMensagem.Servico.IA;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.RateLimiting;
using System.Threading.RateLimiting;

var builder = WebApplication.CreateBuilder(args);

// Render (como a maioria dos PaaS) termina o TLS na borda e repassa pro container em HTTP puro
// (ver ASPNETCORE_URLS=http://+:8080 no .dockerfile) -- sem isso, o app nao sabe que a requisicao
// original era HTTPS e UseHttpsRedirection() entraria em loop de redirecionamento infinito, porque
// internamente toda requisicao chega como HTTP. KnownNetworks/KnownProxies limpos porque o IP do
// proxy da Render nao e fixo/conhecido; como o container so e alcancavel atraves desse proxy,
// aceitar o header de qualquer origem aqui e o padrao recomendado para esse tipo de hospedagem.
builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
    options.KnownNetworks.Clear();
    options.KnownProxies.Clear();
});

builder.Services.AddHsts(options =>
{
    options.MaxAge = TimeSpan.FromDays(365);
    options.IncludeSubDomains = true;
});

// A producao roda atras de Cloudflare -> Render (dois hops de proxy, confirmado pelo header
// "Server: cloudflare" nas respostas). CF-Connecting-IP e a fonte confiavel do IP real do
// cliente nesse caso -- a Cloudflare sempre sobrescreve esse header na borda, entao o cliente
// nao consegue forja-lo. Contar hops de X-Forwarded-For exigiria saber o numero exato de
// proxies na frente (variavel/desconhecido) e ainda seria falsificavel pelo proprio cliente
// antes de chegar na Cloudflare. Sem Cloudflare (dev local), cai no RemoteIpAddress normal.
static string ResolveClientIp(HttpContext context)
{
    var cfConnectingIp = context.Request.Headers["CF-Connecting-IP"].FirstOrDefault();
    return !string.IsNullOrEmpty(cfConnectingIp)
        ? cfConnectingIp
        : context.Connection.RemoteIpAddress?.ToString() ?? "desconhecido";
}

builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

    // Limite global por IP -- rede de seguranca basica contra scraping/DoS sem atrapalhar uso normal.
    options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(context =>
        RateLimitPartition.GetFixedWindowLimiter(
            partitionKey: ResolveClientIp(context),
            factory: _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = 100,
                Window = TimeSpan.FromMinutes(1),
                QueueLimit = 0
            }));

    // Login e "esqueci minha senha" sao alvo classico de forca bruta/enumeracao de email --
    // limite bem mais apertado, tambem por IP. O login hoje faz BCrypt.Verify sem nenhum lockout
    // ou contagem de tentativas, entao isso e a unica barreira contra tentativa exaustiva de senha.
    options.AddPolicy("auth", context =>
        RateLimitPartition.GetFixedWindowLimiter(
            partitionKey: ResolveClientIp(context),
            factory: _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = 5,
                Window = TimeSpan.FromMinutes(1),
                QueueLimit = 0
            }));
});

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
// Sem fallback: um segredo hardcoded no codigo-fonte anula a assinatura do token
// (qualquer um com acesso ao repo poderia forjar um JWT valido). Falha no startup
// e melhor que subir "funcionando" com uma chave publica.
var secretKey = jwtSettings["SecretKey"]
    ?? throw new InvalidOperationException("JwtSettings:SecretKey nao configurado. Defina essa configuracao (appsettings ou variavel de ambiente) antes de iniciar a API.");

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
builder.Services.Configure<GeminiConfiguration>(builder.Configuration.GetSection("GeminiConfiguration"));
builder.Services.AddHttpClient<IGeminiService, GeminiService>();
// AddScoped, nao AddHttpClient: a classe consome IHttpClientFactory.CreateClient() diretamente
// (mesmo padrao do WebhookDispatcherService.cs), nao recebe HttpClient tipado no construtor.
builder.Services.AddScoped<IWebhookDispatcherService, ProjetoMetaMensagem.Servico.Webhook.WebhookDispatcherService>();

// Worker de disparo de campanhas agendadas. Estava implementado (CampanhaWorker.cs) mas nunca
// registrado aqui -- campanhas criadas via /api/campanha/incluir ficavam presas em "AGENDADA"
// para sempre, sem nenhum processo rodando pra efetivamente enviar as mensagens.
builder.Services.AddHostedService<CampanhaWorker>();

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
builder.Services.AddScoped<IAssinaturaRepository, AssinaturaRepository>();
builder.Services.AddScoped<IOrigemLeadRepository, OrigemLeadRepository>();
builder.Services.AddScoped<INotificadorChat, ProjetoMetaMensagem.WebAPI.Hubs.NotificadorChat>();
builder.Services.Configure<ProjetoMetaMensagem.Servico.Configuration.MetaConversoesConfiguration>(
    builder.Configuration.GetSection("MetaConversoesConfiguration"));
builder.Services.AddHttpClient<IConversoesMetaService, ProjetoMetaMensagem.Servico.Meta.ConversoesMetaService>(client =>
{
    client.BaseAddress = new Uri("https://graph.facebook.com/v19.0/");
});
builder.Services.AddSingleton<IConfiguracaoOfertasCakto, ProjetoMetaMensagem.Servico.Cobranca.ConfiguracaoOfertasCakto>();
builder.Services.Configure<ProjetoMetaMensagem.Servico.Configuration.PlanosConfiguration>(
    builder.Configuration.GetSection("PlanosConfiguration"));
builder.Services.AddScoped<IOnboardingComercialService, ProjetoMetaMensagem.Servico.Cobranca.OnboardingComercialService>();
// Sequencia de nascer uma conta de cliente, compartilhada pelo webhook da Cakto e pelo
// cadastro interno feito pela equipe.
builder.Services.AddScoped<ICriacaoDeContaDeCliente, ProjetoMetaMensagem.Dominio.Servicos.CriacaoDeContaDeCliente>();

// Flow Orchestrator
builder.Services.AddScoped<IFlowOrchestratorService, FlowOrchestratorService>();

//Inje��o de depend�ncia de Mensagens e disparos do Core
// Todos os IRequestHandler do Dominio sao registrados por varredura automatica.
// Antes eram 85 linhas manuais, e esquecer uma so quebrava em runtime -- ver
// RegistroDeHandlers.cs e o teste que garante que nenhum comando fica sem handler.
builder.Services.AddHandlersDoDominio();


//Estrutura de Login e esqueci minha senha montada
builder.Services.AddScoped<IDashboardRepository, DashboardRepository>();

//Registros de Empresas


//Registros de Usuario

//Registros de Numero

//Registros de Template


//Registros de Contato

//Registros de Mensagem

//Registros de Mensagem




//Registros de Convcrsations


// Registros de Tag
// Estes handlers existiam e estavam expostos por controllers, mas nunca foram registrados aqui --
// como o Mediator resolve por DI, todo endpoint dessas areas respondia 500 em runtime
// ("No service for type IRequestHandler<...>"), sem nunca falhar no build.

// Registros de Produto

// Registros de IA (assistente generico usado por Chats/Templates/Flows/Disparador)

// Registros de WebhookConfig

// Registros de Campanha

// Registros de Pipeline (CRM)

// Registros de Etapa do Pipeline

// Registros de Lead do Pipeline

var app = builder.Build();

// Precisa ser o primeiro middleware do pipeline: tudo que le esquema (https/http) ou IP do
// cliente abaixo (HSTS, HttpsRedirection, o RemoteIpAddress usado no rate limiter) depende
// do X-Forwarded-Proto/X-Forwarded-For ja terem sido aplicados a Request antes de chegar neles.
app.UseForwardedHeaders();

app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Projeto Meta Mensagem V1");
    // Se quiser que abra direto ao iniciar, deixe vazio.
    // Se preferir acessar via /swagger, comente a linha abaixo.
    c.RoutePrefix = string.Empty;
});



app.UseCors("AllowReactApp");

if (!app.Environment.IsDevelopment())
{
    app.UseHsts();
}
app.UseHttpsRedirection();

// Headers de seguranca basicos. CSP fica de fora de proposito: essa API so serve JSON (+
// Swagger), quem serve HTML pro usuario final e o Angular hospedado a parte -- um CSP aqui
// nao protegeria nada relevante e quebraria o Swagger UI (usa script/style inline).
app.Use(async (context, next) =>
{
    context.Response.Headers.Append("X-Content-Type-Options", "nosniff");
    context.Response.Headers.Append("X-Frame-Options", "DENY");
    context.Response.Headers.Append("Referrer-Policy", "strict-origin-when-cross-origin");
    await next();
});

app.UseMiddleware<ProjetoMetaMensagem.WebAPI.Common.ValidacaoAssinaturaMetaMiddleware>();

app.UseAuthentication();
app.UseAuthorization();

app.UseRateLimiter();

app.MapControllers();
// O front conecta em `${environment.apiUrl}/hubs/chat`, e apiUrl ja inclui "/api" --
// sem o prefixo aqui, o negotiate batia 404 (rota nao existia onde o front chamava).
app.MapHub<ChatHub>("/api/hubs/chat");

app.Run();
