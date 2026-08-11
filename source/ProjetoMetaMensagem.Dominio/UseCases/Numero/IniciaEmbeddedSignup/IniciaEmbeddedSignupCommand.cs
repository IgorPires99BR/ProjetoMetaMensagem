using ProjetoMetaMensagem.Dominio.Common;
using ProjetoMetaMensagem.Dominio.Interfaces.Mediator;
using System;

namespace ProjetoMetaMensagem.Dominio.UseCases.Numero.IniciaEmbeddedSignup
{
    public class IniciaEmbeddedSignupCommand : IRequest<Response<IniciaEmbeddedSignupResult>>
    {
        public Guid UsuarioId { get; set; }
        public Guid IdEmpresa { get; set; }

        public string Code { get; set; }

        public string NumeroTelefone { get; set; }
        public string? NomeEmpresa { get; set; }

        // Vem do evento "message" (WA_EMBEDDED_SIGNUP) que a Meta dispara no browser durante
        // o fluxo -- e o unico jeito de saber o phone_number_id/waba_id atribuido ao numero,
        // ja que a troca de code por token (TrocarCodeEmbeddedSignupAsync) nao devolve isso.
        public string? PhoneNumberId { get; set; }
        public string? WabaId { get; set; }
    }
}
