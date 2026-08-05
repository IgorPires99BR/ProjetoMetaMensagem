using ProjetoMetaMensagem.Dominio.Common;
using ProjetoMetaMensagem.Dominio.Interfaces.Mediator;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjetoMetaMensagem.Dominio.UseCases.Auth.EsqueceuASenha
{
    public class EsqueceuASenhaCommand : IRequest<Response<EsqueceuASenhaResult>>
    {
        public string? company_id { get; set; }
        public string Email { get; set; } = string.Empty;
    }
}
