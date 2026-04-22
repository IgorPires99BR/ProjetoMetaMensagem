using ProjetoMetaMensagem.Dominio.Common;
using ProjetoMetaMensagem.Dominio.Interfaces.Mediator;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjetoMetaMensagem.Dominio.UseCases.Usuario.CriaUsuario
{
    public class CriaUsuarioCommand : IRequest<Response<CriaUsuarioResult>>
    {
        public string EmpresaId { get; set; }

        public string Nome { get; set; }

        public string? Email { get; set; }

        public string? SenhaHash { get; set; }
    }
}
