using ProjetoMetaMensagem.Dominio.Common;
using ProjetoMetaMensagem.Dominio.Interfaces.Mediator;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjetoMetaMensagem.Dominio.UseCases.Usuario.AlteraUsuario
{
    public class AlteraUsuarioCommand : IRequest<Response<AlteraUsuarioResult>>
    {
        public Guid Id { get; set; }
        public string EmpresaId { get; set; }

        public string Nome { get; set; }

        public string? Email { get; set; }

        public string? SenhaHash { get; set; }
    }
}
