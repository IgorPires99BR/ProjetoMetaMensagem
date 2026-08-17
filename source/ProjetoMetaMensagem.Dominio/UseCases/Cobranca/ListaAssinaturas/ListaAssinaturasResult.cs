using ProjetoMetaMensagem.Dominio.Entidades;
using System;
using System.Collections.Generic;

namespace ProjetoMetaMensagem.Dominio.UseCases.Cobranca.ListaAssinaturas
{
    public class ListaAssinaturasResult
    {
        public List<AssinaturaResumo> Assinaturas { get; set; } = new();

        // Totais só fazem sentido para quem opera a plataforma; para o cliente, sempre a dele.
        public int TotalAtivas { get; set; }
        public decimal ReceitaMensalEstimada { get; set; }
    }

    public class AssinaturaResumo
    {
        public AssinaturaResumo() { }

        public AssinaturaResumo(Assinatura assinatura, string? nomeEmpresa)
        {
            Id = assinatura.Id;
            EmpresaId = assinatura.EmpresaId;
            NomeEmpresa = nomeEmpresa ?? "(empresa removida)";
            EmailComprador = assinatura.EmailComprador;
            Plano = assinatura.Plano;
            Status = assinatura.Status;
            Valor = assinatura.ValorCentavos.HasValue ? assinatura.ValorCentavos.Value / 100m : (decimal?)null;
            DataInicio = assinatura.DataInicio;
            DataProximaCobranca = assinatura.DataProximaCobranca;
            DataCancelamento = assinatura.DataCancelamento;
            UltimoEvento = assinatura.UltimoEvento;
            DataUltimoEvento = assinatura.DataUltimoEvento;
        }

        public Guid Id { get; set; }
        public Guid EmpresaId { get; set; }
        public string NomeEmpresa { get; set; } = string.Empty;
        public string? EmailComprador { get; set; }
        public string Plano { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public decimal? Valor { get; set; }
        public DateTime DataInicio { get; set; }
        public DateTime? DataProximaCobranca { get; set; }
        public DateTime? DataCancelamento { get; set; }
        public string? UltimoEvento { get; set; }
        public DateTime? DataUltimoEvento { get; set; }
    }
}
