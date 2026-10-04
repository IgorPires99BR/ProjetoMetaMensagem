using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using ProjetoMetaMensagem.Dominio.Entidades;
using ProjetoMetaMensagem.Dominio.Interfaces;
using ProjetoMetaMensagem.Dominio.Interfaces.Servicos;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace ProjetoMetaMensagem.Servico.Meta
{
    public class CredenciaisMetaDaPlataforma : ICredenciaisMetaDaPlataforma
    {
        // Fallback quando CaktoConfiguration:EmpresaOperacaoId nao esta configurado (ex: banco
        // local). Mesmo criterio usado no Y03 para montar a Sebrecon.
        private const string NomeEmpresaDaPlataforma = "Contact Solution";

        private readonly IUnitOfWork _unitOfWork;
        private readonly IConfiguration _configuration;
        private readonly ILogger<CredenciaisMetaDaPlataforma> _logger;

        public CredenciaisMetaDaPlataforma(IUnitOfWork unitOfWork, IConfiguration configuration, ILogger<CredenciaisMetaDaPlataforma> logger)
        {
            _unitOfWork = unitOfWork;
            _configuration = configuration;
            _logger = logger;
        }

        public async Task PreencherEmBranco(Empresa empresa)
        {
            var plataforma = await ObterEmpresaDaPlataforma();

            if (plataforma == null)
            {
                // Nao bloqueia o cadastro: a empresa so fica sem disparar ate alguem configurar.
                _logger.LogWarning("Empresa {Nome} criada sem credenciais Meta: empresa da plataforma (Contact Solution) nao encontrada.", empresa.Nome);
                return;
            }

            if (string.IsNullOrWhiteSpace(empresa.WabaId)) empresa.WabaId = plataforma.WabaId;
            if (string.IsNullOrWhiteSpace(empresa.PhoneNumberId)) empresa.PhoneNumberId = plataforma.PhoneNumberId;
            if (string.IsNullOrWhiteSpace(empresa.MetaAccessToken)) empresa.MetaAccessToken = plataforma.MetaAccessToken;
            if (string.IsNullOrWhiteSpace(empresa.AppIdMeta)) empresa.AppIdMeta = plataforma.AppIdMeta;
        }

        private async Task<Empresa?> ObterEmpresaDaPlataforma()
        {
            if (Guid.TryParse(_configuration["CaktoConfiguration:EmpresaOperacaoId"], out var id))
            {
                var porId = await _unitOfWork.Empresa.ObterPorId(id);
                if (porId != null) return porId;
            }

            var empresas = await _unitOfWork.Empresa.Obter();
            return empresas.FirstOrDefault(e => string.Equals(e.Nome?.Trim(), NomeEmpresaDaPlataforma, StringComparison.OrdinalIgnoreCase));
        }
    }
}
