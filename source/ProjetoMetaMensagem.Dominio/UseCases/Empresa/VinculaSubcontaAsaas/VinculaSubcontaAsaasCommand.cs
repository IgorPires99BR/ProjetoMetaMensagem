using ProjetoMetaMensagem.Dominio.Common;
using ProjetoMetaMensagem.Dominio.Interfaces.Mediator;
using System;

namespace ProjetoMetaMensagem.Dominio.UseCases.Empresa.VinculaSubcontaAsaas
{
    // Subconta aberta pelo painel do Asaas (fora desta API): so o walletId e informado, o resto
    // vem do proprio Asaas.
    public class VinculaSubcontaAsaasCommand : IRequest<Response<VinculaSubcontaAsaasResult>>
    {
        public Guid EmpresaId { get; set; }
        public string? WalletId { get; set; }
    }
}
