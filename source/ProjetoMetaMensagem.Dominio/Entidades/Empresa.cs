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
            public Empresa(CriaEmpresaCommand command)
            {
                Id = command.Id;
                Nome = command.Nome;
                Email = command.Email;
                Telefone = command.Telefone;
                DataCriacao = DateTime.Now;
            }

            [Key]
            public string Id { get; set; } = Guid.NewGuid().ToString();

            [Required]
            [MaxLength(255)]
            public string Nome { get; set; }

            [MaxLength(255)]
            public string? Email { get; set; }

            [MaxLength(50)]
            public string? Telefone { get; set; }

            public DateTimeOffset DataCriacao { get; set; } = DateTimeOffset.Now;
        
    }
}
