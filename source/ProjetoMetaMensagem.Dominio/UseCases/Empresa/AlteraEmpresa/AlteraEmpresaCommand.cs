using ProjetoMetaMensagem.Dominio.Common;
using ProjetoMetaMensagem.Dominio.Interfaces.Mediator;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjetoMetaMensagem.Dominio.UseCases.Empresa.AlteraEmpresa
{
    public class AlteraEmpresaCommand : IRequest<Response<AlteraEmpresaResult>>
    {
        public Guid Id { get; set; }
        public string Nome { get; set; } = string.Empty;
        public string? Email { get; set; }
        public string Telefone { get; set; } = string.Empty;
        public string Cnpj { get; set; } = string.Empty;
        public string AccessToken { get; set; } = string.Empty;
        public string? WabaId { get; set; }
        public string? PhoneNumberId { get; set; }
        public string PlanoId { get; set; } = string.Empty;
    }
}
