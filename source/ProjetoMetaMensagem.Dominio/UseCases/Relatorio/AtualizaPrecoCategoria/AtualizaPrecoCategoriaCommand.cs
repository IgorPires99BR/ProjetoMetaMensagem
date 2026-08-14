using ProjetoMetaMensagem.Dominio.Common;
using ProjetoMetaMensagem.Dominio.Interfaces.Mediator;

namespace ProjetoMetaMensagem.Dominio.UseCases.Relatorio.AtualizaPrecoCategoria
{
    public class AtualizaPrecoCategoriaCommand : IRequest<Response<ObtemPrecoCategoria.ObtemPrecoCategoriaResult>>
    {
        public bool SolicitanteEhAdmin { get; set; }

        public string Categoria { get; set; } = string.Empty;
        public decimal PrecoUnitario { get; set; }
    }
}
