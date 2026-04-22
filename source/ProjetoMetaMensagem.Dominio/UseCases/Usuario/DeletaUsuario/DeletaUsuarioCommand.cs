using ProjetoMetaMensagem.Dominio.Common;
using ProjetoMetaMensagem.Dominio.Interfaces.Mediator;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjetoMetaMensagem.Dominio.UseCases.Usuario.DeletaUsuario
{
    public class DeletaUsuarioCommand : IRequest<Response<DeletaUsuarioResult>>
    {
        public Guid Id { get; set; }
    }
}
