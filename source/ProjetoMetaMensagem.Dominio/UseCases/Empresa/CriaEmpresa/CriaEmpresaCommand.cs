using ProjetoMetaMensagem.Dominio.Common;
using ProjetoMetaMensagem.Dominio.Interfaces.Mediator;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjetoMetaMensagem.Dominio.UseCases.Empresa.CriaEmpresa
{
    public class CriaEmpresaCommand : IRequest<Response<CriaEmpresaResult>>
    {
        public string Id { get; set; }
        public string Nome{ get; set; }
        public string Email{ get; set; }
        public string Telefone{ get; set; }
    }
}
