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
        public string UsuarioId { get; set; }
        public string Telefone { get; set; }
        public string? Nome { get; set; }
        public string? Email { get; set; }

        public DateTimeOffset DataCriacao { get; set; }
    }
}
