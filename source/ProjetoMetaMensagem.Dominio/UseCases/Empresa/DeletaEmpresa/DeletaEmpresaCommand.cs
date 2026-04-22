using ProjetoMetaMensagem.Dominio.Common;
using ProjetoMetaMensagem.Dominio.Interfaces.Mediator;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjetoMetaMensagem.Dominio.UseCases.Empresa.DeletaEmpresa
{
    public class DeletaEmpresaCommand : IRequest<Response<DeletaEmpresaResult>>
    {
        public string IdEmpresa { get; set; }
    }
}
