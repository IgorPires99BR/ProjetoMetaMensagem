using Microsoft.Extensions.Options;
using ProjetoMetaMensagem.Dominio.Interfaces.Servicos;
using ProjetoMetaMensagem.Servico.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ProjetoMetaMensagem.Servico.Cobranca
{
    public class ProvedoresCobranca : IProvedoresCobranca
    {
        private readonly Dictionary<string, IProvedorCobranca> _porNome;
        private readonly string _ativo;

        public ProvedoresCobranca(IEnumerable<IProvedorCobranca> provedores, IOptions<CobrancaConfiguration> config)
        {
            _porNome = provedores.ToDictionary(p => p.Nome, StringComparer.OrdinalIgnoreCase);
            _ativo = string.IsNullOrWhiteSpace(config.Value.Provedor) ? ProvedorCobranca.MercadoPago : config.Value.Provedor.Trim();
        }

        // Nome invalido em Cobranca:Provedor so aparece aqui (e no log de boot): falha alta em vez
        // de cair calado em outro provedor e cobrar pela conta errada.
        public IProvedorCobranca Ativo => _porNome.TryGetValue(_ativo, out var provedor)
            ? provedor
            : throw new PixException($"Provedor de cobrança \"{_ativo}\" desconhecido em Cobranca:Provedor. Avise o suporte.");

        public IProvedorCobranca Do(string? nome) =>
            _porNome.TryGetValue(string.IsNullOrWhiteSpace(nome) ? ProvedorCobranca.Asaas : nome, out var provedor)
                ? provedor
                : throw new PixException($"Provedor de cobrança \"{nome}\" desconhecido.");
    }
}
