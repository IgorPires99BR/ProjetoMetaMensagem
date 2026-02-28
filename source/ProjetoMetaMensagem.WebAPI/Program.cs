using Microsoft.Extensions.Options;
using ProjetoMetaMensagem.Dominio.Common;
using ProjetoMetaMensagem.Dominio.Interfaces.Mediator;
using ProjetoMetaMensagem.Dominio.Interfaces.Servicos;
using ProjetoMetaMensagem.Dominio.UseCases.EnviarMensagemMeta;
using ProjetoMetaMensagem.Servico.Configuration;
using ProjetoMetaMensagem.Servico.Meta;
using ProjetoMetaMensagem.Servico.WPPConnect;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.Configure<ApiWhatsappConnectionConfiguration>(
    builder.Configuration.GetSection("ApiWhatsappConnection"));

builder.Services.AddHttpClient<WPPConnectService>();

builder.Services.AddScoped<IWhatsappService>(sp =>
{
    var settings = sp
        .GetRequiredService<IOptions<ApiWhatsappConnectionConfiguration>>()
    .Value;

    return settings.Provider switch
    {
        "WPPConnect" => sp.GetRequiredService<WPPConnectService>(),
        _ => throw new NotImplementedException("Provider não configurado")
    };
});
//mediator
builder.Services.AddScoped<IMediator, Mediator>();

//servicos
builder.Services.AddHttpClient<IMetaService, MetaService>();


//repositorios

//UseCases
builder.Services.AddScoped<IRequestHandler<EnviarMensagemMetaCommand, Response<EnviarMensagemMetaResult>>, EnviarMensagemMetaHandler>();


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
