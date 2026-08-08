using ProjetoMetaMensagem.Dominio.Common;
using ProjetoMetaMensagem.Dominio.Interfaces.Mediator;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjetoMetaMensagem.Dominio.UseCases.Contato.AlteraContato
{
    public class AlteraContatoCommand : IRequest<Response<AlteraContatoResult>>
    {
        public Guid Id { get; set; }
        public Guid UsuarioId { get; set; }
        public string Telefone { get; set; }
        public string? Nome { get; set; }
        public string? Email { get; set; }

        public DateTimeOffset DataCriacao { get; set; }

        // Preenchido pelo controller a partir do JWT (null = administrador). Sem isso o UPDATE
        // casava so pelo Id e permitia alterar contato de outra empresa.
        public Guid? EmpresaIdSolicitante { get; set; }
    }
}
