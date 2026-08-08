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
        public Guid EmpresaId { get; set; }

        public string Nome { get; set; }

        public string? Email { get; set; }

        public string? SenhaHash { get; set; }

        // Preenchido pelo controller a partir do JWT (null = administrador). Nao confundir com
        // EmpresaId, que vem do corpo e por isso o atacante escolhe. Sem esse escopo o UPDATE
        // casava so pelo Id e permitia alterar usuario de outra empresa.
        public Guid? EmpresaIdSolicitante { get; set; }
    }
}
