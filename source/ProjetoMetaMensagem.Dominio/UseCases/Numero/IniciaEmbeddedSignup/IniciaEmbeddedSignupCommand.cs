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
    }
}
