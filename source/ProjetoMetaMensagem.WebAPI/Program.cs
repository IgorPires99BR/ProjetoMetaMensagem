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
using ProjetoMetaMensagem.Dominio.UseCases.Numero.AlteraNumero;
using ProjetoMetaMensagem.Dominio.UseCases.Numero.CriaNumero;
using ProjetoMetaMensagem.Dominio.UseCases.Numero.DeletaNumero;
using ProjetoMetaMensagem.Dominio.UseCases.Numero.ObtemNumero;
using ProjetoMetaMensagem.Dominio.UseCases.Usuario.AlteraUsuario;
using ProjetoMetaMensagem.Dominio.UseCases.Usuario.CriaUsuario;
using ProjetoMetaMensagem.Dominio.UseCases.Usuario.DeletaUsuario;
using ProjetoMetaMensagem.Dominio.UseCases.Usuario.ObtemUsuario;
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
            policy.WithOrigins("http://localhost:5173", "http://localhost:3000", "http://localhost:4200") // Portas comuns do React/Vite
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
builder.Services.AddScoped<INumeroRepository, NumeroRepository>();

//Injeção de dependência de Mensagens e disparos do Core
builder.Services.AddScoped<IRequestHandler<EnviarMensagemMetaCommand, Response<EnviarMensagemMetaResult>>, EnviarMensagemMetaHandler>();
builder.Services.AddScoped<IRequestHandler<CriarTemplateMetaCommand, Response<CriarTemplateMetaResult>>, CriarTemplateMetaHandler>();


//Estrutura de Login e esqueci minha senha montada
builder.Services.AddScoped<IRequestHandler<EsqueceuASenhaCommand, Response<EsqueceuASenhaResult>>, EsqueceuASenhaHandler>();
builder.Services.AddScoped<IRequestHandler<LoginCommand, Response<LoginResult>>, LoginHandler>();


//Registros de Companies
builder.Services.AddScoped<IRequestHandler<ProjetoMetaMensagem.Dominio.UseCases.Admin.Companies.DeletaEmpresa.DeletaEmpresaCommand, Response<ProjetoMetaMensagem.Dominio.UseCases.Admin.Companies.DeletaEmpresa.DeletaEmpresaResult>>, ProjetoMetaMensagem.Dominio.UseCases.Admin.Companies.DeletaEmpresa.DeletaEmpresaHandler>();
builder.Services.AddScoped<IRequestHandler<ProjetoMetaMensagem.Dominio.UseCases.Admin.Companies.AlteraEmpresa.AlteraEmpresaCommand, Response<ProjetoMetaMensagem.Dominio.UseCases.Admin.Companies.AlteraEmpresa.AlteraEmpresaResult>>, ProjetoMetaMensagem.Dominio.UseCases.Admin.Companies.AlteraEmpresa.AlteraEmpresaHandler>();
builder.Services.AddScoped<IRequestHandler<ProjetoMetaMensagem.Dominio.UseCases.Admin.Companies.ObtemEmpresa.ObtemEmpresaCommand, Response<List<ProjetoMetaMensagem.Dominio.UseCases.Admin.Companies.ObtemEmpresa.ObtemEmpresaResult>>>, ProjetoMetaMensagem.Dominio.UseCases.Admin.Companies.ObtemEmpresa.ObtemEmpresaHandler>();
builder.Services.AddScoped<IRequestHandler<ProjetoMetaMensagem.Dominio.UseCases.Admin.Companies.CriaEmpresa.CriaEmpresaCommand, Response<ProjetoMetaMensagem.Dominio.UseCases.Admin.Companies.CriaEmpresa.CriaEmpresaResult>>, ProjetoMetaMensagem.Dominio.UseCases.Admin.Companies.CriaEmpresa.CriaEmpresaHandler>();

//Registros de Flows

//Registros de Empresas
builder.Services.AddScoped<IRequestHandler<ProjetoMetaMensagem.Dominio.UseCases.Empresa.CriaEmpresa.CriaEmpresaCommand, Response<ProjetoMetaMensagem.Dominio.UseCases.Empresa.CriaEmpresa.CriaEmpresaResult>>, ProjetoMetaMensagem.Dominio.UseCases.Empresa.CriaEmpresa.CriaEmpresaHandler>();
builder.Services.AddScoped<IRequestHandler<ProjetoMetaMensagem.Dominio.UseCases.Empresa.AlteraEmpresa.AlteraEmpresaCommand, Response<ProjetoMetaMensagem.Dominio.UseCases.Empresa.AlteraEmpresa.AlteraEmpresaResult>>, ProjetoMetaMensagem.Dominio.UseCases.Empresa.AlteraEmpresa.AlteraEmpresaHandler>();
builder.Services.AddScoped<IRequestHandler<ProjetoMetaMensagem.Dominio.UseCases.Empresa.DeletaEmpresa.DeletaEmpresaCommand, Response<ProjetoMetaMensagem.Dominio.UseCases.Empresa.DeletaEmpresa.DeletaEmpresaResult>>, ProjetoMetaMensagem.Dominio.UseCases.Empresa.DeletaEmpresa.DeletaEmpresaHandler>();
builder.Services.AddScoped<IRequestHandler<ProjetoMetaMensagem.Dominio.UseCases.Empresa.ObtemEmpresa.ObtemEmpresaCommand, Response<ProjetoMetaMensagem.Dominio.UseCases.Empresa.ObtemEmpresa.ObtemEmpresaResult>>, ProjetoMetaMensagem.Dominio.UseCases.Empresa.ObtemEmpresa.ObtemEmpresaHandler>();


//Registros de Usuario
builder.Services.AddScoped<IRequestHandler<CriaUsuarioCommand, Response<CriaUsuarioResult>>, CriaUsuarioHandler>();
builder.Services.AddScoped<IRequestHandler<AlteraUsuarioCommand, Response<AlteraUsuarioResult>>, AlteraUsuarioHandler>();
builder.Services.AddScoped<IRequestHandler<DeletaUsuarioCommand, Response<DeletaUsuarioResult>>, DeletaUsuarioHandler>();
builder.Services.AddScoped<IRequestHandler<ObtemUsuarioCommand, Response<List<ObtemUsuarioResult>>>, ObtemUsuarioHandler>();

//Registros de Numero
builder.Services.AddScoped<IRequestHandler<CriaNumeroCommand, Response<CriaNumeroResult>>, CriaNumeroHandler>();
builder.Services.AddScoped<IRequestHandler<AlteraNumeroCommand, Response<AlteraNumeroResult>>, AlteraNumeroHandler>();
builder.Services.AddScoped<IRequestHandler<DeletaNumeroCommand, Response<DeletaNumeroResult>>, DeletaNumeroHandler>();
builder.Services.AddScoped<IRequestHandler<ObtemNumeroCommand, Response<List<ObtemNumeroResult>>>, ObtemNumeroHandler>();

//Registros de Contato
builder.Services.AddScoped<IRequestHandler<CriaContatoCommand, Response<CriaContatoResult>>, CriaContatoHandler>();
builder.Services.AddScoped<IRequestHandler<AlteraContatoCommand, Response<AlteraContatoResult>>, AlteraContatoHandler>();
builder.Services.AddScoped<IRequestHandler<DeletaContatoCommand, Response<DeletaContatoResult>>, DeletaContatoHandler>();
builder.Services.AddScoped<IRequestHandler<ObtemContatoCommand, Response<List<ObtemContatoResult>>>, ObtemContatoHandler>();


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
