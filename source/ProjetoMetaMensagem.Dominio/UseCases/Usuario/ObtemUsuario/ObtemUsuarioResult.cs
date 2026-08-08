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
            DataCriacao = usuario.DataCriacao;

        }

        public Guid Id { get; set; }
        public Guid EmpresaId { get; set; }
        public string Nome { get; set; }
        public string? Email { get; set; }

        // O hash da senha NAO e exposto aqui de proposito: ele vazava no JSON de
        // listagem de usuarios, entregando o hash BCrypt de todo mundo pra qualquer
        // um que chamasse o endpoint. O frontend nunca usou esse campo (a tela de
        // usuarios sempre zera a senha ao editar), entao remover nao quebra nada.

        public DateTime DataCriacao { get; set; }
    }
}
