using ProjetoMetaMensagem.Dominio.Common;
using ProjetoMetaMensagem.Dominio.Interfaces.Mediator;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjetoMetaMensagem.Dominio.UseCases.Auth.Login
{
    public class LoginCommand : IRequest<Response<LoginResult>>
    {
        public string email { get; set; }
        public string password { get; set; }
    }
}
