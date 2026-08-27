using ProjetoMetaMensagem.Dominio.Interfaces.Mediator;
using System.Reflection;

namespace ProjetoMetaMensagem.WebAPI.Common
{
    // Registra sozinho todo IRequestHandler<,> do assembly do Dominio.
    //
    // Antes cada caso de uso novo exigia uma linha manual em Program.cs (eram 85). Esquecer
    // compilava normalmente e so quebrava em runtime, quando alguem usava a tela, com um erro
    // generico de "servico nao encontrado" -- perigoso o bastante pra estar documentado em
    // letras garrafais no guia do projeto, o que resolve pra quem le o guia e nao resolve pra
    // quem nao le.
    public static class RegistroDeHandlers
    {
        public static IServiceCollection AddHandlersDoDominio(this IServiceCollection services)
        {
            // Ancora no proprio IRequest pra achar o assembly do Dominio sem citar o nome dele
            // por string (que quebraria silenciosamente numa renomeacao).
            var assemblyDominio = typeof(IRequest<>).Assembly;

            var registrados = 0;

            foreach (var tipo in assemblyDominio.GetTypes())
            {
                if (tipo.IsAbstract || tipo.IsInterface || tipo.IsGenericTypeDefinition) continue;

                var interfacesDeHandler = tipo.GetInterfaces()
                    .Where(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IRequestHandler<,>));

                foreach (var contrato in interfacesDeHandler)
                {
                    // TryAdd em vez de Add: se alguem registrar um handler especifico a mao
                    // antes daqui (pra trocar a implementacao), essa escolha continua valendo.
                    services.TryAddScopedCompat(contrato, tipo);
                    registrados++;
                }
            }

            if (registrados == 0)
            {
                // Zero handler achado significa que a varredura parou de funcionar -- e a API
                // subiria "normalmente" pra so quebrar no primeiro request. Melhor falhar aqui.
                throw new InvalidOperationException(
                    "Nenhum IRequestHandler encontrado no assembly do Dominio. A varredura automatica de handlers quebrou.");
            }

            return services;
        }

        private static void TryAddScopedCompat(this IServiceCollection services, Type contrato, Type implementacao)
        {
            if (services.Any(d => d.ServiceType == contrato)) return;
            services.AddScoped(contrato, implementacao);
        }
    }
}
