using Newtonsoft.Json;
using ProjetoMetaMensagem.Dominio.Entidades;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjetoMetaMensagem.Dominio.UseCases.Auth.Login
{
    public class LoginResult
    {
        public LoginResult(Companies company)
        {
            //this.CompanyId = company.id;
            //this.IdEmpresa = company.name;
            //this.IdUsuario = company.name;
            this.Email = company.email;

            var admins = new[] { "MASTER", "IGOR_SOCIO", "JOSE_SOCIO" };

            if (admins.Contains(company.id?.ToUpper()))
            {
                this.Role = "admin";
                this.Status = "success";
            }
            else
            {
                this.Role = "client";
                this.Status = "success";
            }
        }

        public LoginResult(Entidades.Usuario usuario, string? token = null)
        {
            IdEmpresa = usuario.EmpresaId;
            IdUsuario = usuario.Id;
            Email = usuario.Email;
            Role = usuario.IsAdmin.HasValue && usuario.IsAdmin.Value ? "admin" : "operador";
            Status = "success";
            Token = token;
        }

        [JsonProperty("status")]
        public string Status { get; set; } = "success";

        [JsonProperty("companyId")]
        public Guid IdEmpresa { get; set; }

        [JsonProperty("companyName")]
        public Guid IdUsuario { get; set; }
        //public string NomeEmpresa { get; set; }

        [JsonProperty("role")]
        public string Role { get; set; } = null!;

        [JsonProperty("email")]
        public string Email { get; set; } = null!;

        [JsonProperty("token")]
        public string? Token { get; set; }
    }
}
