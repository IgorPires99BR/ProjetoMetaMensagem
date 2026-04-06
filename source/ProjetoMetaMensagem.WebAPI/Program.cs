using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using ProjetoMetaMensagem.Data;
using ProjetoMetaMensagem.Data.Repositorios;
using ProjetoMetaMensagem.Dominio.Common;
using ProjetoMetaMensagem.Dominio.Interfaces;
using ProjetoMetaMensagem.Dominio.Interfaces.Mediator;
using ProjetoMetaMensagem.Dominio.Interfaces.Repositorios;
using ProjetoMetaMensagem.Dominio.Interfaces.Servicos;
using ProjetoMetaMensagem.Dominio.UseCases.Admin.Companies.AlteraEmpresa;
using ProjetoMetaMensagem.Dominio.UseCases.Admin.Companies.CriaEmpresa;
using ProjetoMetaMensagem.Dominio.UseCases.Admin.Companies.DeletaEmpresa;
using ProjetoMetaMensagem.Dominio.UseCases.Admin.Companies.ObtemEmpresa;
using ProjetoMetaMensagem.Dominio.UseCases.Auth.EsqueceuASenha;
using ProjetoMetaMensagem.Dominio.UseCases.Auth.Login;
using ProjetoMetaMensagem.Dominio.UseCases.Flows.CriaFlow;
using ProjetoMetaMensagem.Dominio.UseCases.Flows.ListaFlows;
using ProjetoMetaMensagem.Dominio.UseCases.Leads.ListaConversas;
using ProjetoMetaMensagem.Dominio.UseCases.Messages.EnviarMensagemMeta;
using ProjetoMetaMensagem.Servico.Configuration;
using ProjetoMetaMensagem.Servico.Email;
using ProjetoMetaMensagem.Servico.Meta;
using ProjetoMetaMensagem.Servico.Twilio;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowReactApp",
        policy =>
        {
            policy.WithOrigins("http://localhost:5173", "http://localhost:3000") // Portas comuns do React/Vite
                  .AllowAnyHeader()
                  .AllowAnyMethod();
        });
});

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.Configure<ApiWhatsappConnectionConfiguration>(builder.Configuration.GetSection("ApiWhatsappConnectionConfiguration"));

builder.Services.AddHttpClient<TwilioService>();

//mediator
builder.Services.AddScoped<IMediator, Mediator>();
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

//Banco
builder.Services.AddScoped<DbSession>();



//servicos
builder.Services.AddHttpClient<IWhatsappService, TwilioService>();
builder.Services.AddHttpClient<IMetaService, MetaService>();
builder.Services.AddHttpClient<IEmailService, EmailService>();

//Configurações

builder.Services.Configure<GmailConfiguration>(
    builder.Configuration.GetSection("GmailConfig"));


//repositorios

builder.Services.AddScoped<ICompaniesRepository, CompaniesRepository>();
builder.Services.AddScoped<IFlowsRepository, FlowsRepository>();
builder.Services.AddScoped<IConversationsRepository, ConversationsRepository>();
builder.Services.AddScoped<IEmpresaRepository, EmpresaRepository>();
builder.Services.AddScoped<IUsuarioRepository, UsuarioRepository>();
builder.Services.AddScoped<IContatoRepository, ContatoRepository>();
builder.Services.AddScoped<ITemplateRepository, TemplateRepository>();

//Injeção de dependência de Mensagens e disparos do Core
builder.Services.AddScoped<IRequestHandler<EnviarMensagemMetaCommand, Response<EnviarMensagemMetaResult>>, EnviarMensagemMetaHandler>();


//Estrutura de Login e esqueci minha senha montada
builder.Services.AddScoped<IRequestHandler<EsqueceuASenhaCommand, Response<EsqueceuASenhaResult>>, EsqueceuASenhaHandler>();
builder.Services.AddScoped<IRequestHandler<LoginCommand, Response<LoginResult>>, LoginHandler>();


//Registros de Companies
builder.Services.AddScoped<IRequestHandler<DeletaEmpresaCommand, Response<DeletaEmpresaResult>>, DeletaEmpresaHandler>();
builder.Services.AddScoped<IRequestHandler<AlteraEmpresaCommand, Response<AlteraEmpresaResult>>, AlteraEmpresaHandler>();
builder.Services.AddScoped<IRequestHandler<ObtemEmpresaCommand, Response<List<ObtemEmpresaResult>>>, ObtemEmpresaHandler>();
builder.Services.AddScoped<IRequestHandler<CriaEmpresaCommand, Response<CriaEmpresaResult>>, CriaEmpresaHandler>();

//Registros de Flows

builder.Services.AddScoped<IRequestHandler<ListaFlowsCommand, Response<List<ListaFlowsResult>>>, ListaFlowsHandler>();
builder.Services.AddScoped<IRequestHandler<CriaFlowCommand, Response<CriaFlowResult>>, CriaFlowHandler>();

//Registros de Convcrsations

builder.Services.AddScoped<IRequestHandler<ListaConversaPorIdCommand, Response<List<ListaConversaPorIdResult>>>, ListaConversaPorIdHandler>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("AllowReactApp");

//app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
