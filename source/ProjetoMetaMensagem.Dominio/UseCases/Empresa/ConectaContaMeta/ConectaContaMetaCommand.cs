using ProjetoMetaMensagem.Dominio.Common;
using ProjetoMetaMensagem.Dominio.Interfaces.Mediator;
using System;

namespace ProjetoMetaMensagem.Dominio.UseCases.Empresa.ConectaContaMeta
{
    // Provisiona a propria conta Meta de um cliente novo (Embedded Signup a nivel de Empresa,
    // nao de Numero -- ver IniciaEmbeddedSignupCommand pro fluxo que so conecta mais um numero
    // numa empresa que ja tem WabaId/PhoneNumberId/MetaAccessToken proprios). So a conta de
    // plataforma pode rodar isto: e quem cadastra cliente novo.
    public class ConectaContaMetaCommand : IRequest<Response<ConectaContaMetaResult>>
    {
        public Guid EmpresaId { get; set; }
        public string Code { get; set; }

        // Vem do evento "message" (WA_EMBEDDED_SIGNUP) que a Meta dispara no browser --
        // mesmo mecanismo do fluxo de Numero, ver numeros.component.ts.
        public string? PhoneNumberId { get; set; }
        public string? WabaId { get; set; }

        public bool SolicitanteEhAdminDaPlataforma { get; set; }
    }
}
