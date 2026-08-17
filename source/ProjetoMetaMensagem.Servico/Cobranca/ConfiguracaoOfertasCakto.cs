using Microsoft.Extensions.Configuration;
using ProjetoMetaMensagem.Dominio.Interfaces.Servicos;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ProjetoMetaMensagem.Servico.Cobranca
{
    // Lê o mapa de ofertas do appsettings/variáveis de ambiente:
    //
    //   "CaktoConfiguration": { "Ofertas": { "B8BcHrY": "PRO", "K2xLm9": "STARTER" } }
    //
    // Sem configuração, devolve null e o handler cai na leitura pelo nome da oferta.
    public class ConfiguracaoOfertasCakto : IConfiguracaoOfertasCakto
    {
        private readonly Dictionary<string, string> _ofertas;

        public ConfiguracaoOfertasCakto(IConfiguration configuration)
        {
            _ofertas = configuration
                .GetSection("CaktoConfiguration:Ofertas")
                .GetChildren()
                .Where(item => !string.IsNullOrWhiteSpace(item.Value))
                .ToDictionary(item => item.Key, item => item.Value!.ToUpperInvariant(), StringComparer.OrdinalIgnoreCase);
        }

        public string? PlanoDaOferta(string ofertaId)
        {
            if (string.IsNullOrWhiteSpace(ofertaId)) return null;

            return _ofertas.TryGetValue(ofertaId, out var plano) ? plano : null;
        }
    }
}
