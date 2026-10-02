using ProjetoMetaMensagem.Dominio.Common;
using ProjetoMetaMensagem.Dominio.Interfaces.Mediator;
using System;

namespace ProjetoMetaMensagem.Dominio.UseCases.Numero.AplicaNomeNumero
{
    // Depois que a Meta aprova o novo nome, ele só aparece pro cliente quando o número é
    // registrado de novo (POST /register) -- é o que este caso de uso faz.
    public class AplicaNomeNumeroCommand : IRequest<Response<AplicaNomeNumeroResult>>
    {
        public Guid NumeroId { get; set; }

        // Escopo sempre vindo do token (nunca do corpo/rota)
        public Guid? EmpresaIdSolicitante { get; set; }

        // PIN de verificação em 2 etapas do número na Meta (6 dígitos), exigido pelo /register
        public string Pin { get; set; }
    }
}
