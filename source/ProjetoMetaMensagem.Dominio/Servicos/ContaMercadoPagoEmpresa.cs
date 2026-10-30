using ProjetoMetaMensagem.Dominio.Entidades;
using ProjetoMetaMensagem.Dominio.Interfaces.Servicos;

namespace ProjetoMetaMensagem.Dominio.Servicos
{
    public static class ContaMercadoPagoEmpresa
    {
        // Token da conta Mercado Pago da empresa para consultar/cancelar as orders feitas nela.
        // Nulo = empresa desconectou: a order fica sem baixa automatica (a consulta loga o 401/404).
        public static string? Token(DadosBancariosEmpresa? dados, ICriptografiaService criptografia) =>
            dados?.MercadoPagoConectado == true ? criptografia.Descriptografar(dados.MercadoPagoAccessTokenCriptografado!) : null;
    }
}
