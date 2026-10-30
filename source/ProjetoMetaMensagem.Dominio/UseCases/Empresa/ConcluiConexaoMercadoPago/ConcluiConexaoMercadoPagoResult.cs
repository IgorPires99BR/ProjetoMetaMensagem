using System;

namespace ProjetoMetaMensagem.Dominio.UseCases.Empresa.ConcluiConexaoMercadoPago
{
    public class ConcluiConexaoMercadoPagoResult
    {
        public Guid EmpresaId { get; set; }
        public string MercadoPagoUserId { get; set; } = string.Empty;
    }
}
