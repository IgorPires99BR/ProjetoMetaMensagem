using ProjetoMetaMensagem.Dominio.Entidades;
using System;

namespace ProjetoMetaMensagem.Dominio.UseCases.Empresa.ObtemDadosBancarios
{
    public class ObtemDadosBancariosResult
    {
        // Empresa que ainda nao cadastrou nada recebe o registro vazio, pra tela abrir o
        // formulario em vez de tratar "nao encontrado". A apiKey da subconta nunca sai daqui.
        public ObtemDadosBancariosResult(Guid empresaId, DadosBancariosEmpresa? dados)
        {
            dados ??= new DadosBancariosEmpresa { EmpresaId = empresaId };

            EmpresaId = empresaId;
            AsaasContaId = dados.AsaasContaId;
            AsaasWalletId = dados.AsaasWalletId;
            AsaasContaCriadaEm = dados.AsaasContaCriadaEm;
            CarteiraExterna = dados.CarteiraExterna;
            ComissaoTipo = dados.ComissaoTipo;
            ComissaoValor = dados.ComissaoValor;
            CobrancaPixAtiva = dados.CobrancaPixAtiva;
            DataAtualizacao = dados.DataAtualizacao ?? (dados.DataCriacao == default ? null : dados.DataCriacao);
        }

        public Guid EmpresaId { get; set; }
        public string? AsaasContaId { get; set; }
        public string? AsaasWalletId { get; set; }
        public DateTime? AsaasContaCriadaEm { get; set; }
        public bool TemSubconta => !string.IsNullOrWhiteSpace(AsaasWalletId);
        public bool CarteiraExterna { get; set; }
        public string? ComissaoTipo { get; set; }
        public decimal? ComissaoValor { get; set; }
        public bool CobrancaPixAtiva { get; set; }
        public DateTime? DataAtualizacao { get; set; }
    }
}
