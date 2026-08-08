using ProjetoMetaMensagem.Dominio.Common;
using ProjetoMetaMensagem.Dominio.Interfaces.Mediator;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjetoMetaMensagem.Dominio.UseCases.Contato.CriaContato
{
    public class CriaContatoCommand : IRequest<Response<CriaContatoResult>>
    {
        public Guid UsuarioId { get; set; }
        public string Telefone { get; set; }
        public string? Nome { get; set; }
        public string? Email { get; set; }

        public DateTimeOffset DataCriacao { get; set; }

        // Preenchido pelo controller a partir do JWT (null = administrador). Sem isso, o
        // UsuarioId do corpo definia sozinho a empresa do contato (via Usuario.EmpresaId) e o
        // EmpresaAccessFilter nao tem como saber que "UsuarioId" e um id de empresa por tabela:
        // um usuario comum de outra empresa injetava contato na empresa alheia mandando o id de
        // um usuario de la.
        public Guid? EmpresaIdSolicitante { get; set; }
    }
}
