using ProjetoMetaMensagem.Dominio.UseCases.Empresa.CriaEmpresa;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjetoMetaMensagem.Dominio.Entidades
{
    public class Empresa
    {
        public Empresa()
        {

        }

        public Empresa(CriaEmpresaCommand command)
        {
            Nome = command.Nome;
            Email = command.Email;
            Cnpj = command.Cnpj;
            Telefone = command.Telefone;
            MetaAccessToken = command.AccessToken;
            PlanoId = command.PlanoId;
            DataCriacao = DateTime.Now;
        }

        [Key]
        public Guid Id { get; set; }

        [Required]
        [MaxLength(255)]
        public string Nome { get; set; }

        [MaxLength(255)]
        public string? Email { get; set; }
        public string Cnpj { get; set; }

        [MaxLength(50)]
        public string? Telefone { get; set; }
        public string? WabaId { get; set; }
        public string? MetaAccessToken { get; set; }
        public string? PhoneNumberId { get; set; }
        public string? AppIdMeta { get; set; }
        public string? StatusAccount { get; set; }
        public Guid? TokenWebhookLocal { get; set; }
        public string? PlanoId { get; set; }

        public DateTimeOffset? DataCriacao { get; set; } = DateTimeOffset.Now;

    }
}
