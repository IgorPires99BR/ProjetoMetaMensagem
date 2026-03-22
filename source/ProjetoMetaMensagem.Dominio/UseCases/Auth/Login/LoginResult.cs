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
            Status = "success";
            CompanyId = "Master";
            CompanyName = "Administrador Geral";
            Role = "admin";
            Email = company.email;
        }

        [JsonProperty("status")]
        public string Status { get; set; } = "success";

        [JsonProperty("companyId")]
        public string CompanyId { get; set; } = null!;

        [JsonProperty("companyName")]
        public string CompanyName { get; set; } = null!;

        [JsonProperty("role")]
        public string Role { get; set; } = null!;

        [JsonProperty("email")]
        public string Email { get; set; } = null!;
    }
}
