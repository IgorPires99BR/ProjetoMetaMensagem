using ProjetoMetaMensagem.Dominio.Common;
using ProjetoMetaMensagem.Dominio.Interfaces.Mediator;
using System;

namespace ProjetoMetaMensagem.Dominio.UseCases.CobrancaCliente.SimulaPagamentoSandbox
{
    // So com o Asaas em SANDBOX: paga o Pix de mentira e ja roda a baixa. Rodando local, o
    // webhook do Asaas nao alcanca a maquina e a consulta periodica demora ate 10 minutos.
    public class SimulaPagamentoSandboxCommand : IRequest<Response<SimulaPagamentoSandboxResult>>
    {
        public Guid CobrancaClienteId { get; set; }

        // Escopo sempre vindo do token (nunca do corpo/rota).
        public Guid? EmpresaIdSolicitante { get; set; }
    }
}
