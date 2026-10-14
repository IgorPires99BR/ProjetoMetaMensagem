using ProjetoMetaMensagem.Dominio.Entidades;
using System;

namespace ProjetoMetaMensagem.Dominio.Servicos
{
    // Divide o valor cobrado entre a plataforma (comissao) e a empresa (repasse). O split vai
    // para o Asaas em valor fixo, ja calculado aqui: o percentual do Asaas incide sobre o valor
    // LIQUIDO (depois da taxa dele) e a comissao gravada deixaria de bater com o extrato.
    // A taxa do Asaas sai da parte da plataforma -- ver AsaasConfiguration.TaxaPix.
    public static class CalculoComissao
    {
        public record Resultado(decimal Comissao, decimal Repasse);

        // Null quando o repasse nao seria positivo (comissao fixa maior que a fatura, por exemplo).
        public static Resultado? Calcular(decimal valorCobrado, string comissaoTipo, decimal comissaoValor)
        {
            var comissao = comissaoTipo == DadosBancariosEmpresa.ComissaoPercentual
                ? Math.Round(valorCobrado * comissaoValor / 100m, 2, MidpointRounding.AwayFromZero)
                : Math.Round(comissaoValor, 2, MidpointRounding.AwayFromZero);

            var repasse = valorCobrado - comissao;
            return repasse > 0m ? new Resultado(comissao, repasse) : null;
        }
    }
}
