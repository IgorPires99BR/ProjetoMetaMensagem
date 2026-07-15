using ProjetoMetaMensagem.Dominio.Common;
using ProjetoMetaMensagem.Dominio.Interfaces.Mediator;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjetoMetaMensagem.Dominio.UseCases.MensagemRecebida.ListaChatsAtivos
{
    public class ListaChatsAtivosCommand : IRequest<Response<ListaChatsAtivosResult>>
    {
        public ListaChatsAtivosCommand(Guid idEmpresa, Guid? idContato)
        {
            IdEmpresa = idEmpresa;
            IdContato = idContato;
        }
        public Guid IdEmpresa { get; set; }
        public Guid? IdContato { get; set; }
    }
}
