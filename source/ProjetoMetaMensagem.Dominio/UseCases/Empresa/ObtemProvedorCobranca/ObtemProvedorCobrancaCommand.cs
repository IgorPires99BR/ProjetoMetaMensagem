using ProjetoMetaMensagem.Dominio.Common;
using ProjetoMetaMensagem.Dominio.Interfaces.Mediator;
using System;

namespace ProjetoMetaMensagem.Dominio.UseCases.Empresa.ObtemProvedorCobranca
{
    // Por qual adquirente sai o Pix desta empresa agora, para as telas de disparo mostrarem ao
    // lado das variaveis de Pix. Qualquer usuario da empresa (quem dispara nem sempre e admin):
    // so devolve nomes, nada dos dados bancarios.
    public class ObtemProvedorCobrancaCommand : IRequest<Response<ObtemProvedorCobrancaResult>>
    {
        public ObtemProvedorCobrancaCommand(Guid empresaId) => EmpresaId = empresaId;

        public Guid EmpresaId { get; set; }
    }
}
