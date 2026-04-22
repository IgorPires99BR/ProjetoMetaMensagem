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
            this.CompanyName = company.name;
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

        public LoginResult(Entidades.Usuario usuario)
        {
            CompanyId = usuario.EmpresaId;
            Email = usuario.Email;
            this.Role = "admin";
            this.Status = "success";
        }

        [JsonProperty("status")]
        public string Status { get; set; } = "success";

        [JsonProperty("companyId")]
        public Guid CompanyId { get; set; }

        [JsonProperty("companyName")]
        public string CompanyName { get; set; } = null!;

        [JsonProperty("role")]
        public string Role { get; set; } = null!;

        [JsonProperty("email")]
        public string Email { get; set; } = null!;
    }
}
