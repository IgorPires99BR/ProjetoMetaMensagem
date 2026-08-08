using ProjetoMetaMensagem.Dominio.Common;
using ProjetoMetaMensagem.Dominio.Interfaces.Mediator;

namespace ProjetoMetaMensagem.Dominio.UseCases.Campanha.CancelaCampanha
{
    public class CancelaCampanhaCommand : IRequest<Response<CancelaCampanhaResult>>
    {
        public Guid Id { get; set; }

        // Preenchido pelo controller a partir do JWT (null = administrador, enxerga tudo).
        // Sem esse escopo o UPDATE era feito so por Id, e um usuario de outra empresa
        // conseguia cancelar campanha alheia mandando o id na rota.
        public Guid? EmpresaIdSolicitante { get; set; }
    }
}
