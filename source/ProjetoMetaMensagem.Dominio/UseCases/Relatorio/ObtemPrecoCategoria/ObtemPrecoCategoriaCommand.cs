using ProjetoMetaMensagem.Dominio.Common;
using ProjetoMetaMensagem.Dominio.Interfaces.Mediator;

namespace ProjetoMetaMensagem.Dominio.UseCases.Relatorio.ObtemPrecoCategoria
{
    public class ObtemPrecoCategoriaCommand : IRequest<Response<ObtemPrecoCategoriaResult>>
    {
        public bool SolicitanteEhAdmin { get; set; }
    }
}
