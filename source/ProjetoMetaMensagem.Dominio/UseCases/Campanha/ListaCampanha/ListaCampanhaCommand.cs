using ProjetoMetaMensagem.Dominio.Common;
using ProjetoMetaMensagem.Dominio.Interfaces.Mediator;
using System.Collections.Generic;

namespace ProjetoMetaMensagem.Dominio.UseCases.Campanha.ListaCampanha
{
    public class ListaCampanhaCommand : IRequest<Response<List<ListaCampanhaResult>>>
    {
        public Guid EmpresaId { get; set; }
    }
}
