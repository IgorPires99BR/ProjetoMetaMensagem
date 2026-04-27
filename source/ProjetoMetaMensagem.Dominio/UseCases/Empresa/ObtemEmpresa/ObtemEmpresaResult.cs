using ProjetoMetaMensagem.Dominio.Entidades;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjetoMetaMensagem.Dominio.UseCases.Empresa.ObtemEmpresa
{
    public class ObtemEmpresaResult
    {
        public ObtemEmpresaResult(Entidades.Empresa empresa)
        {
            Id = empresa.Id;
            Nome = empresa.Nome;
            Email = empresa.Email;
            Telefone = empresa.Telefone;
            DataCriacao = empresa.DataCriacao;
        }

        public Guid Id { get; set; } 
        public string Nome { get; set; }
        public string? Email { get; set; }
        public string? Telefone { get; set; }
        public DateTimeOffset DataCriacao { get; set; }

    }
}
