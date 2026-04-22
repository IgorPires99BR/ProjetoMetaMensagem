using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjetoMetaMensagem.Dominio.UseCases.Contato.ObtemContato
{
    public class ObtemContatoResult
    {
        public ObtemContatoResult(Entidades.Contato contato)
        {
            Id = contato.Id;
            UsuarioId = contato.UsuarioId;
            Telefone = contato.Telefone;
            Nome = contato.Nome;
            Email = contato.Email;
            DataCriacao = contato.DataCriacao;
        }
        public Guid Id { get; set; }
        public Guid UsuarioId { get; set; }
        public string Telefone { get; set; }
        public string? Nome { get; set; }
        public string? Email { get; set; }
        public DateTime DataCriacao { get; set; }
    }
}
