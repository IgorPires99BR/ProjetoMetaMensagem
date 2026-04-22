using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjetoMetaMensagem.Dominio.UseCases.Usuario.ObtemUsuario
{
    public class ObtemUsuarioResult
    {
        public ObtemUsuarioResult(Entidades.Usuario usuario)
        {
            Id = usuario.Id;
            EmpresaId = usuario.EmpresaId;
            Nome = usuario.Nome;
            Email = usuario.Email;
            SenhaHash = usuario.SenhaHash;
            DataCriacao = usuario.DataCriacao;
            
        }

        public Guid Id { get; set; }
        public Guid EmpresaId { get; set; }
        public string Nome { get; set; }
        public string? Email { get; set; }

        public string? SenhaHash { get; set; }

        public DateTime DataCriacao { get; set; }
    }
}
