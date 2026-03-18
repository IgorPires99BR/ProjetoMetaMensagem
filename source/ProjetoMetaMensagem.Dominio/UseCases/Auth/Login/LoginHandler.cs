using ProjetoMetaMensagem.Dominio.Common;
using ProjetoMetaMensagem.Dominio.Interfaces.Mediator;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjetoMetaMensagem.Dominio.UseCases.Auth.Login
{
    public class LoginHandler : IRequestHandler<LoginCommand, Response<LoginResult>>
    {
        public async Task<Response<LoginResult>> Handle(LoginCommand request)
        {
            throw new NotImplementedException();
        }
    }
}
