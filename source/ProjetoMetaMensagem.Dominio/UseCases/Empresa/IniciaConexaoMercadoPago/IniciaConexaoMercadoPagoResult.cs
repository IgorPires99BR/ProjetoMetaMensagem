using System;

namespace ProjetoMetaMensagem.Dominio.UseCases.Empresa.IniciaConexaoMercadoPago
{
    public class IniciaConexaoMercadoPagoResult
    {
        // Pagina de autorizacao do Mercado Pago; o front redireciona o navegador para ela.
        public string UrlAutorizacao { get; set; } = string.Empty;
    }
}
