using ProjetoMetaMensagem.Dominio.Entidades;
using ProjetoMetaMensagem.Dominio.Servicos;
using System;

namespace ProjetoMetaMensagem.Dominio.UseCases.Empresa.ObtemDadosBancarios
{
    public class ObtemDadosBancariosResult
    {
        // Empresa que ainda nao cadastrou nada recebe o registro vazio, pra tela abrir o
        // formulario em vez de tratar "nao encontrado". A apiKey da subconta e os tokens do
        // Mercado Pago nunca saem daqui.
        public ObtemDadosBancariosResult(Guid empresaId, DadosBancariosEmpresa? dados)
        {
            var destino = DestinoRecebimento.Resolver(dados, HorarioBrasilia.Agora());
            dados ??= new DadosBancariosEmpresa { EmpresaId = empresaId };

            EmpresaId = empresaId;
            ProvedorRecebimento = dados.ProvedorRecebimento;
            MercadoPagoConectado = dados.MercadoPagoConectado;
            MercadoPagoUserId = dados.MercadoPagoUserId;
            MercadoPagoConectadoEm = dados.MercadoPagoConectadoEm;
            MercadoPagoTokenExpiraEm = dados.MercadoPagoTokenExpiraEm;
            AsaasContaId = dados.AsaasContaId;
            AsaasWalletId = dados.AsaasWalletId;
            AsaasContaCriadaEm = dados.AsaasContaCriadaEm;
            CarteiraExterna = dados.CarteiraExterna;
            ComissaoTipo = dados.ComissaoTipo;
            ComissaoValor = dados.ComissaoValor;
            RecebimentoNaPlataforma = dados.RecebimentoNaPlataforma;
            CobrancaPixAtiva = dados.CobrancaPixAtiva;
            DataAtualizacao = dados.DataAtualizacao ?? (dados.DataCriacao == default ? null : dados.DataCriacao);
            RecebimentoNaContaDaEmpresa = !destino.NaPlataforma;
            MotivoRecebimentoNaPlataforma = destino.MotivoPlataforma;
        }

        public Guid EmpresaId { get; set; }

        // MERCADOPAGO ou ASAAS: a conta (uma so) onde a empresa quer receber.
        public string ProvedorRecebimento { get; set; }

        public bool MercadoPagoConectado { get; set; }
        public string? MercadoPagoUserId { get; set; }
        public DateTime? MercadoPagoConectadoEm { get; set; }
        public DateTime? MercadoPagoTokenExpiraEm { get; set; }

        public string? AsaasContaId { get; set; }
        public string? AsaasWalletId { get; set; }
        public DateTime? AsaasContaCriadaEm { get; set; }
        public bool TemSubconta => !string.IsNullOrWhiteSpace(AsaasWalletId);
        public bool CarteiraExterna { get; set; }
        public string? ComissaoTipo { get; set; }
        public decimal? ComissaoValor { get; set; }
        public bool RecebimentoNaPlataforma { get; set; }

        // Onde as cobrancas desta empresa caem HOJE (DestinoRecebimento): conta dela, com a
        // comissao para a plataforma, ou conta da Contact Solution -- com o motivo, pra tela dizer
        // o que falta.
        public bool RecebimentoNaContaDaEmpresa { get; set; }
        public string? MotivoRecebimentoNaPlataforma { get; set; }

        // Provedor das cobrancas da propria Contact Solution (Cobranca:Provedor). So na consulta.
        public string? ProvedorAtivo { get; set; }
        public bool CobrancaPixAtiva { get; set; }
        public DateTime? DataAtualizacao { get; set; }
    }
}
