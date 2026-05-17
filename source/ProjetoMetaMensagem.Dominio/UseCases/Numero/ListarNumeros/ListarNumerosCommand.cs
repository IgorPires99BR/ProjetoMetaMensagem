using ProjetoMetaMensagem.Dominio.Common;
using ProjetoMetaMensagem.Dominio.Interfaces.Mediator;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjetoMetaMensagem.Dominio.UseCases.Numero.ListarNumeros
{
    public class ListarNumerosCommand : IRequest<Response<List<ListarNumerosResult>>>
    {
        public ListarNumerosCommand(Guid idUsuario)
        {
            IdUsuario = idUsuario;
        }
        public Guid IdUsuario{ get; set; }
    }
}
