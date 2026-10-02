using ProjetoMetaMensagem.Dominio.Common;
using ProjetoMetaMensagem.Dominio.Interfaces.Mediator;
using System;

namespace ProjetoMetaMensagem.Dominio.UseCases.Numero.ObtemPerfilNumero
{
    public class ObtemPerfilNumeroCommand : IRequest<Response<ObtemPerfilNumeroResult>>
    {
        public Guid NumeroId { get; set; }

        // Escopo sempre vindo do token (nunca do corpo/rota)
        public Guid? EmpresaIdSolicitante { get; set; }
    }
}
