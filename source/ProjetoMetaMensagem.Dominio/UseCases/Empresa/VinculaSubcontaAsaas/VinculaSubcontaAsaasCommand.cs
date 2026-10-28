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

        // True = conta Asaas propria da empresa, fora da plataforma. O Asaas nao tem como
        // confirmar de quem e a carteira: quem vincula responde por ter recebido o walletId do titular.
        public bool CarteiraExterna { get; set; }

        // True = troca a conta que ja recebe o repasse (ex: Sebrecon saindo da conta Asaas propria
        // para uma subconta). Pix ja gerados mantem o split antigo -- o Asaas fixa na criacao.
        public bool Substituir { get; set; }
    }
}
