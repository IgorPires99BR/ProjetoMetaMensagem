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
using ProjetoMetaMensagem.Dominio.UseCases.Messages.EnviarMensagemMeta;
using ProjetoMetaMensagem.Servico.Configuration;
using ProjetoMetaMensagem.Servico.Meta;
using ProjetoMetaMensagem.Servico.Twilio;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.Configure<ApiWhatsappConnectionConfiguration>(
    builder.Configuration.GetSection("ApiWhatsappConnection"));

builder.Services.AddHttpClient<TwilioService>();

//mediator
builder.Services.AddScoped<IMediator, Mediator>();
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

//Banco
builder.Services.AddScoped<DbSession>();

//servicos
builder.Services.AddHttpClient<IWhatsappService, TwilioService>();
builder.Services.AddHttpClient<IMetaService, MetaService>();


//repositorios

builder.Services.AddScoped<ICompaniesRepository, CompaniesRepository>();

//UseCases
builder.Services.AddScoped<IRequestHandler<EnviarMensagemMetaCommand, Response<EnviarMensagemMetaResult>>, EnviarMensagemMetaHandler>();
builder.Services.AddScoped<IRequestHandler<EsqueceuASenhaCommand, Response<EsqueceuASenhaResult>>, EsqueceuASenhaHandler>();
builder.Services.AddScoped<IRequestHandler<LoginCommand, Response<LoginResult>>, LoginHandler>();


var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
