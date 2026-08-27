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
            IsAdmin = EhPerfilAdmin(command.Perfil);
            DataCriacao = DateTime.Now;
        }

        public Usuario(AlteraUsuarioCommand command)
        {
            Id = command.Id;
            EmpresaId = command.EmpresaId;
            Nome = command.Nome;
            Email = command.Email;
            SenhaHash = command.SenhaHash;
            // Perfil ausente na edicao significa "nao mexer no perfil", nao "rebaixar": a
            // redefinicao de senha e qualquer chamada parcial nao podem tirar o admin de
            // alguem sem querer. O UPDATE trata null como "manter o valor atual".
            IsAdmin = command.Perfil is null ? null : EhPerfilAdmin(command.Perfil);
            DataCriacao = DateTime.Now;
        }

        // O banco so tem o booleano IsAdmin; a tela fala em "perfil". A traducao mora aqui pra
        // que as duas pontas nao divirjam de novo. Perfil ausente cai em operador -- o menos
        // privilegiado -- porque quem nao escolheu nao pediu acesso total.
        public static bool EhPerfilAdmin(string? perfil) =>
            string.Equals(perfil, PerfilAdmin, StringComparison.OrdinalIgnoreCase);

        public const string PerfilAdmin = "admin";
        public const string PerfilOperador = "operador";

        public Guid Id { get; set; }
        public Guid EmpresaId { get; set; }
        public string Nome { get; set; }
        public string? Email { get; set; }
        public bool? IsAdmin { get; set; }

        public string? SenhaHash { get; set; }

        public DateTime DataCriacao { get; set; }
    }
}
