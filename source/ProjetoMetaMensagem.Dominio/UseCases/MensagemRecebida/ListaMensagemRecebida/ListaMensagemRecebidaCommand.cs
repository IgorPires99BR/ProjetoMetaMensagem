using ProjetoMetaMensagem.Dominio.Common;
using ProjetoMetaMensagem.Dominio.Interfaces.Mediator;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjetoMetaMensagem.Dominio.UseCases.MensagemRecebida.ListaMensagemRecebida
{
    public class ListaMensagemRecebidaCommand : IRequest<Response<ListaMensagemRecebidaResult>>
    {
        public Guid EmpresaId{ get; set; }
        public Guid ContatoId{ get; set; }

        // Paginacao pro infinite scroll (0 = pagina mais recente)
        public int Pagina { get; set; } = 0;
        public int TamanhoPagina { get; set; } = 30;
    }
}
