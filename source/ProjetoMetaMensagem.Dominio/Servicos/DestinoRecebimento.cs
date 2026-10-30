using ProjetoMetaMensagem.Dominio.Entidades;
using ProjetoMetaMensagem.Dominio.Interfaces.Servicos;
using System;

namespace ProjetoMetaMensagem.Dominio.Servicos
{
    public enum TipoDestinoRecebimento
    {
        // Conta da Contact Solution, no provedor ativo (Cobranca:Provedor), sem divisao.
        Plataforma,
        // Order criada com o token da conta Mercado Pago da empresa; a comissao vai no marketplace_fee.
        ContaMercadoPagoEmpresa,
        // Pix na conta Asaas da plataforma com split do valor menos a comissao para a subconta.
        SubcontaAsaas
    }

    // Para onde vai o dinheiro de uma cobranca da empresa. Regra combinada com o Igor (2026-10-30):
    // a empresa escolhe UMA conta (Mercado Pago ou Asaas); preenchida e com comissao definida, a
    // cobranca e em nome dela e a plataforma fica so com a comissao. Faltando qualquer dado, a
    // cobranca e da propria Contact Solution -- nunca deixa de sair por isso.
    public record DestinoRecebimento(TipoDestinoRecebimento Tipo, string? MotivoPlataforma)
    {
        // Folga para nao usar um token que vence no meio do disparo; o job renova bem antes.
        private static readonly TimeSpan FolgaToken = TimeSpan.FromHours(1);

        public bool NaPlataforma => Tipo == TipoDestinoRecebimento.Plataforma;

        // Provedor do destino da empresa; nulo na plataforma (vale o provedor ativo).
        public string? Provedor => Tipo switch
        {
            TipoDestinoRecebimento.ContaMercadoPagoEmpresa => ProvedorCobranca.MercadoPago,
            TipoDestinoRecebimento.SubcontaAsaas => ProvedorCobranca.Asaas,
            _ => null
        };

        public static DestinoRecebimento Resolver(DadosBancariosEmpresa? dados, DateTime agora)
        {
            if (dados == null) return Plataforma("a empresa não tem dados bancários cadastrados");
            if (dados.RecebimentoNaPlataforma) return Plataforma("recebimento na conta da Contact Solution marcado pela plataforma");

            if (dados.ProvedorRecebimento == ProvedorCobranca.Asaas)
            {
                if (!dados.TemSubconta) return Plataforma("a empresa ainda não tem subconta Asaas");
                if (!dados.ComissaoDefinida) return Plataforma("a comissão da plataforma ainda não foi definida");
                return new DestinoRecebimento(TipoDestinoRecebimento.SubcontaAsaas, null);
            }

            if (!dados.MercadoPagoConectado) return Plataforma("a empresa ainda não conectou a conta Mercado Pago");
            if (dados.MercadoPagoTokenExpiraEm.HasValue && dados.MercadoPagoTokenExpiraEm.Value < agora + FolgaToken)
                return Plataforma("a autorização do Mercado Pago da empresa venceu; conecte a conta de novo");
            if (!dados.ComissaoDefinida) return Plataforma("a comissão da plataforma ainda não foi definida");
            return new DestinoRecebimento(TipoDestinoRecebimento.ContaMercadoPagoEmpresa, null);
        }

        private static DestinoRecebimento Plataforma(string motivo) => new(TipoDestinoRecebimento.Plataforma, motivo);
    }
}
