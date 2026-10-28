using System;

namespace ProjetoMetaMensagem.Dominio.UseCases.CobrancaCliente.SimulaPagamentoSandbox
{
    public class SimulaPagamentoSandboxResult
    {
        public Guid CobrancaClienteId { get; set; }

        // Status do Asaas depois da simulacao (RECEIVED = pago) e o da cobranca aqui depois da
        // baixa. Os dois juntos mostram em que ponto a cadeia parou, se nao chegar a PAGA.
        public string StatusAsaas { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
    }
}
