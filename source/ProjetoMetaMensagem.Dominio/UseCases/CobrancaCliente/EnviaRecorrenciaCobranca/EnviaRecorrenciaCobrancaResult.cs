using System;

namespace ProjetoMetaMensagem.Dominio.UseCases.CobrancaCliente.EnviaRecorrenciaCobranca
{
    public class EnviaRecorrenciaCobrancaResult
    {
        public Guid CobrancaClienteId { get; set; }
        public int TotalEnviados { get; set; }
        public int TotalFalhas { get; set; }
        public bool PixRenovado { get; set; }
        public decimal? ValorCobrado { get; set; }
    }
}
