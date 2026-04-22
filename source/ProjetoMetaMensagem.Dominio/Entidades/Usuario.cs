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
            Id = Guid.NewGuid().ToString();
            EmpresaId = command.EmpresaId;
            Nome = command.Nome;
            Email = command.Email;
            SenhaHash = command.SenhaHash;
            DataCriacao = DateTime.Now;
        }

        public Usuario(AlteraUsuarioCommand command)
        {
            Id = command.Id.ToString();
            EmpresaId = command.EmpresaId;
            Nome = command.Nome;
            Email = command.Email;
            SenhaHash = command.SenhaHash;
            DataCriacao = DateTime.Now;
        }

        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string EmpresaId { get; set; }
        public Empresa Empresa { get; set; }
        public string Nome { get; set; }
        public string? Email { get; set; }

        public string? SenhaHash { get; set; }

        public DateTimeOffset DataCriacao { get; set; } = DateTimeOffset.Now;
    }
}
