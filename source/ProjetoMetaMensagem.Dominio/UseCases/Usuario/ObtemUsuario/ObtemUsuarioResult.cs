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
            // A listagem nunca devolveu o perfil, entao a coluna "Perfil" da tela de Usuarios
            // ficava em branco e a edicao voltava sempre com "Administrador" pre-selecionado --
            // que era justamente o valor errado, ja que o gravado era operador.
            Perfil = usuario.IsAdmin == true ? Entidades.Usuario.PerfilAdmin : Entidades.Usuario.PerfilOperador;
            DataCriacao = usuario.DataCriacao;

        }

        public Guid Id { get; set; }
        public Guid EmpresaId { get; set; }
        public string Nome { get; set; }
        public string? Email { get; set; }
        public string Perfil { get; set; } = Entidades.Usuario.PerfilOperador;

        // O hash da senha NAO e exposto aqui de proposito: ele vazava no JSON de
        // listagem de usuarios, entregando o hash BCrypt de todo mundo pra qualquer
        // um que chamasse o endpoint. O frontend nunca usou esse campo (a tela de
        // usuarios sempre zera a senha ao editar), entao remover nao quebra nada.

        public DateTime DataCriacao { get; set; }
    }
}
