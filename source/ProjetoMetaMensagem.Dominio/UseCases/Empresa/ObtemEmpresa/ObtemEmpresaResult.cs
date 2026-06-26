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
            Cnpj = empresa.Cnpj;
            AccessToken = empresa.MetaAccessToken;
            PhoneNumberId = empresa.PhoneNumberId;
            PlanoId = empresa.PlanoId;
            WabaId = empresa.WabaId;
            Telefone = empresa.Telefone;
            DataCriacao = empresa.DataCriacao.Value;
        }

        public Guid Id { get; set; } 
        public string Nome { get; set; }
        public string? Email { get; set; }
        public string? Telefone { get; set; }
        public string? Cnpj { get; set; }
        public string? AccessToken { get; set; }
        public string? PhoneNumberId { get; set; }
        public string? WabaId { get; set; }
        public string? PlanoId { get; set; }
        public DateTimeOffset DataCriacao { get; set; }

    }
}
