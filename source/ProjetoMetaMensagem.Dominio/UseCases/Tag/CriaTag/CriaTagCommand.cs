using ProjetoMetaMensagem.Dominio.Common;
using ProjetoMetaMensagem.Dominio.Interfaces.Mediator;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjetoMetaMensagem.Dominio.UseCases.Tag.CriaTag
{
    public class CriaTagCommand : IRequest<Response<CriaTagResult>>
    {
        public Guid EmpresaId { get; set; }
        public string Nome { get; set; }
        public string? Cor { get; set; }
    }
}
