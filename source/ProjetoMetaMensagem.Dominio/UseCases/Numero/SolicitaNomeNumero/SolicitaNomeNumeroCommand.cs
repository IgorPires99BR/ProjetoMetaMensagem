using ProjetoMetaMensagem.Dominio.Common;
using ProjetoMetaMensagem.Dominio.Interfaces.Mediator;
using System;

namespace ProjetoMetaMensagem.Dominio.UseCases.Numero.SolicitaNomeNumero
{
    // Pede à Meta a troca do nome que o cliente final vê. Não vale na hora: a Meta revisa e,
    // depois de aprovado, o nome só passa a aparecer com AplicaNomeNumero (novo registro).
    public class SolicitaNomeNumeroCommand : IRequest<Response<SolicitaNomeNumeroResult>>
    {
        public Guid NumeroId { get; set; }

        // Escopo sempre vindo do token (nunca do corpo/rota)
        public Guid? EmpresaIdSolicitante { get; set; }

        public string NovoNome { get; set; }
    }
}
