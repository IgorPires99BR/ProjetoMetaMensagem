using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
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
using ProjetoMetaMensagem.Dominio.UseCases.Numero.ListarNumeros;
using ProjetoMetaMensagem.Dominio.UseCases.Template.CriaTemplate;
using ProjetoMetaMensagem.Dominio.UseCases.Template.DeletaTemplate;
using ProjetoMetaMensagem.Dominio.UseCases.Template.ListaTemplate;
using ProjetoMetaMensagem.Dominio.UseCases.Template.AtualizaTemplateMeta;
using ProjetoMetaMensagem.Dominio.UseCases.Messages.EnviarMensagemTemplateMetaLote;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowReactApp",
        policy =>
        {
            policy.WithOrigins("http://localhost:5173", "http://localhost:3000", "http://localhost:4200", "https://angularcontact.vercel.app") // Portas comuns do React/Vite
                  .AllowAnyHeader()
                  .AllowAnyMethod()
                  .AllowAnyOrigin();
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

//Registros de Empresas
builder.Services.AddScoped<IRequestHandler<CriaEmpresaCommand, Response<CriaEmpresaResult>>, CriaEmpresaHandler>();
builder.Services.AddScoped<IRequestHandler<AlteraEmpresaCommand, Response<AlteraEmpresaResult>>, AlteraEmpresaHandler>();
builder.Services.AddScoped<IRequestHandler<DeletaEmpresaCommand, Response<DeletaEmpresaResult>>, DeletaEmpresaHandler>();
builder.Services.AddScoped<IRequestHandler< ObtemEmpresaCommand, Response<List<ObtemEmpresaResult>>>, ObtemEmpresaHandler>();


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
builder.Services.AddScoped<IRequestHandler<AlteraContatoCommand, Response<AlteraContatoResult>>, AlteraContatoHandler>();
builder.Services.AddScoped<IRequestHandler<DeletaContatoCommand, Response<DeletaContatoResult>>, DeletaContatoHandler>();
builder.Services.AddScoped<IRequestHandler<ObtemContatoCommand, Response<List<ObtemContatoResult>>>, ObtemContatoHandler>();

//Registros de Mensagem
builder.Services.AddScoped<IRequestHandler<EnviarMensagemTemplateMetaCommand, Response<EnviarMensagemTemplateMetaResult>>, EnviarMensagemTemplateMetaHandler>();
builder.Services.AddScoped<IRequestHandler<EnviarMensagemTemplateMetaLoteCommand, Response<EnviarMensagemTemplateMetaLoteResult>>, EnviarMensagemTemplateMetaLoteHandler>();


builder.Services.AddScoped<IRequestHandler<ListaFlowsCommand, Response<List<ListaFlowsResult>>>, ListaFlowsHandler>();
builder.Services.AddScoped<IRequestHandler<CriaFlowCommand, Response<CriaFlowResult>>, CriaFlowHandler>();

//Registros de Convcrsations

builder.Services.AddScoped<IRequestHandler<ListaConversaPorIdCommand, Response<List<ListaConversaPorIdResult>>>, ListaConversaPorIdHandler>();

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

app.UseAuthorization();

app.MapControllers();

app.Run();
