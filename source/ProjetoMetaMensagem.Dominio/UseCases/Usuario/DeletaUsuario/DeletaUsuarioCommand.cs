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
        public DeletaUsuarioCommand(Guid id)
        {
            Id = id;
        }
        public Guid Id { get; set; }

        // Preenchido pelo controller a partir do JWT (null = administrador, enxerga tudo).
        // Sem esse escopo o DELETE era feito so por Id, e um usuario de outra empresa
        // conseguia apagar usuario alheio mandando o id na rota.
        public Guid? EmpresaIdSolicitante { get; set; }
    }
}
