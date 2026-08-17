using ProjetoMetaMensagem.Dominio.Common;
using ProjetoMetaMensagem.Dominio.Interfaces.Mediator;
using System;

namespace ProjetoMetaMensagem.Dominio.UseCases.Cobranca.ListaAssinaturas
{
    public class ListaAssinaturasCommand : IRequest<Response<ListaAssinaturasResult>>
    {
        public ListaAssinaturasCommand(Guid? empresaIdSolicitante, bool ehAdminPlataforma)
        {
            EmpresaIdSolicitante = empresaIdSolicitante;
            EhAdminPlataforma = ehAdminPlataforma;
        }

        // Escopo vem do token: o admin do cliente vê só a assinatura da empresa dele; a operação
        // da Contact Solution vê todas. Sem isso, um cliente leria o faturamento dos outros.
        public Guid? EmpresaIdSolicitante { get; }
        public bool EhAdminPlataforma { get; }
    }
}
