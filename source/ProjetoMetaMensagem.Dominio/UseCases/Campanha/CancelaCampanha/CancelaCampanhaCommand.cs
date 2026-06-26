using ProjetoMetaMensagem.Dominio.Common;
using ProjetoMetaMensagem.Dominio.Interfaces.Mediator;

namespace ProjetoMetaMensagem.Dominio.UseCases.Campanha.CancelaCampanha
{
    public class CancelaCampanhaCommand : IRequest<Response<CancelaCampanhaResult>>
    {
        public Guid Id { get; set; }
    }
}
