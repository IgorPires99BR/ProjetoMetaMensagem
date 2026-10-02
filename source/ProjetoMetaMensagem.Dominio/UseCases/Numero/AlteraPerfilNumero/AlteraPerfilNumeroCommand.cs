using ProjetoMetaMensagem.Dominio.Common;
using ProjetoMetaMensagem.Dominio.Interfaces.Mediator;
using System;
using System.Collections.Generic;

namespace ProjetoMetaMensagem.Dominio.UseCases.Numero.AlteraPerfilNumero
{
    // Textos do perfil do WhatsApp Business. A foto e o nome exibido têm casos de uso próprios:
    // a foto exige upload antes, e o nome passa por revisão da Meta.
    public class AlteraPerfilNumeroCommand : IRequest<Response<AlteraPerfilNumeroResult>>
    {
        public Guid NumeroId { get; set; }

        // Escopo sempre vindo do token (nunca do corpo/rota)
        public Guid? EmpresaIdSolicitante { get; set; }

        public string? Sobre { get; set; }
        public string? Descricao { get; set; }
        public string? Endereco { get; set; }
        public string? Email { get; set; }
        public List<string>? Sites { get; set; }
        public string? Segmento { get; set; }
    }
}
