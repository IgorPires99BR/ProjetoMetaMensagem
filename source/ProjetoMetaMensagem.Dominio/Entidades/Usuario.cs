using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ProjetoMetaMensagem.Dominio.UseCases.Usuario.CriaUsuario;
using ProjetoMetaMensagem.Dominio.UseCases.Usuario.AlteraUsuario;

namespace ProjetoMetaMensagem.Dominio.Entidades
{
    public class Usuario
    {
        public Usuario()
        {
            
        }

        public Usuario(CriaUsuarioCommand command)
        {
            EmpresaId = command.EmpresaId;
            Nome = command.Nome;
            Email = command.Email;
            SenhaHash = command.SenhaHash;
            DataCriacao = DateTime.Now;
        }

        public Usuario(AlteraUsuarioCommand command)
        {
            Id = command.Id;
            EmpresaId = command.EmpresaId;
            Nome = command.Nome;
            Email = command.Email;
            SenhaHash = command.SenhaHash;
            DataCriacao = DateTime.Now;
        }

        public Guid Id { get; set; }
        public Guid EmpresaId { get; set; }
        public string Nome { get; set; }
        public string? Email { get; set; }
        public bool? IsAdmin { get; set; }

        public string? SenhaHash { get; set; }

        public DateTime DataCriacao { get; set; }
    }
}
