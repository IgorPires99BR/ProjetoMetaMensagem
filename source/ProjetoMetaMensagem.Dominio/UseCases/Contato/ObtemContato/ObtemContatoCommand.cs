using ProjetoMetaMensagem.Dominio.Common;
using ProjetoMetaMensagem.Dominio.Interfaces.Mediator;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjetoMetaMensagem.Dominio.UseCases.Contato.ObtemContato
{
    public class ObtemContatoCommand : IRequest<Response<List<ObtemContatoResult>>>
    {
        public ObtemContatoCommand(Guid empresaId)
        {
            IdEmpresa = empresaId;
        }
        public Guid IdEmpresa { get; set; }
    }
}
