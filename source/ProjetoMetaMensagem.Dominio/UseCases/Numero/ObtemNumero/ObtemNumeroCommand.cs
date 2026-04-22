using ProjetoMetaMensagem.Dominio.Common;
using ProjetoMetaMensagem.Dominio.Interfaces.Mediator;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjetoMetaMensagem.Dominio.UseCases.Numero.ObtemNumero
{
    public class ObtemNumeroCommand : IRequest<Response<List<ObtemNumeroResult>>>
    {
        public ObtemNumeroCommand(string idEmpresa)
        {
            IdEmpresa = Guid.Parse(idEmpresa);
        }
        public Guid IdEmpresa { get; set; }
    }
}
