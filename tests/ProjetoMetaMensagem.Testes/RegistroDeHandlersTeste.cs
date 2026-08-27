using Microsoft.Extensions.DependencyInjection;
using ProjetoMetaMensagem.Dominio.Interfaces.Mediator;
using ProjetoMetaMensagem.WebAPI.Common;
using Xunit;

namespace ProjetoMetaMensagem.Testes;

// Cada caso de uso novo exigia uma linha manual de registro em Program.cs. Esquecer compilava
// normalmente e so quebrava em runtime, quando alguem usava a tela, com "servico nao
// encontrado" -- erro que nao diz qual handler falta nem de qual tela veio.
//
// Agora os handlers sao varridos automaticamente. Estes testes protegem a varredura: se ela
// parar de achar algum handler, falha aqui e nao em producao.
public class RegistroDeHandlersTeste
{
    [Fact]
    public void Todo_comando_do_dominio_tem_handler_registrado()
    {
        var servicos = new ServiceCollection();
        servicos.AddHandlersDoDominio();

        var assemblyDominio = typeof(IRequest<>).Assembly;

        // Todo comando concreto que implementa IRequest<TResponse> precisa de alguem que o
        // trate -- e o contrato que o mediator resolve na hora do request.
        var comandos = assemblyDominio.GetTypes()
            .Where(t => !t.IsAbstract && !t.IsInterface && !t.IsGenericTypeDefinition)
            .SelectMany(t => t.GetInterfaces()
                .Where(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IRequest<>))
                .Select(i => (Comando: t, Resposta: i.GetGenericArguments()[0])))
            .ToList();

        var registrados = servicos
            .Select(d => d.ServiceType)
            .Where(t => t.IsGenericType && t.GetGenericTypeDefinition() == typeof(IRequestHandler<,>))
            .Select(t => t.GetGenericArguments()[0])
            .ToHashSet();

        // Common.Request e uma classe vazia herdada do esqueleto do mediator, sem nenhum uso --
        // nao e um caso de uso de verdade e por isso nao precisa de handler. Qualquer OUTRO
        // comando sem handler e bug.
        var semHandler = comandos
            .Where(c => !registrados.Contains(c.Comando))
            .Where(c => c.Comando != typeof(Dominio.Common.Request))
            .Select(c => c.Comando.Name)
            .OrderBy(n => n)
            .ToList();

        Assert.True(
            semHandler.Count == 0,
            $"Comandos sem handler registrado: {string.Join(", ", semHandler)}");
    }

    [Fact]
    public void Varredura_encontra_uma_quantidade_plausivel_de_handlers()
    {
        var servicos = new ServiceCollection();
        servicos.AddHandlersDoDominio();

        var quantidade = servicos.Count(d =>
            d.ServiceType.IsGenericType &&
            d.ServiceType.GetGenericTypeDefinition() == typeof(IRequestHandler<,>));

        // O registro manual que a varredura substituiu tinha 85 linhas. Um numero muito abaixo
        // disso significa que a varredura parou de funcionar -- e a API subiria "normalmente"
        // pra quebrar no primeiro request.
        Assert.True(quantidade >= 80, $"Varredura achou so {quantidade} handlers; esperado ao menos 80.");
    }
}
