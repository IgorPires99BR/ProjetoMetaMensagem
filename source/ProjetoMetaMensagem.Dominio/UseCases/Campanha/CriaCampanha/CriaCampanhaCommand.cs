using ProjetoMetaMensagem.Dominio.Common;
using ProjetoMetaMensagem.Dominio.Interfaces.Mediator;

namespace ProjetoMetaMensagem.Dominio.UseCases.Campanha.CriaCampanha
{
    public class CriaCampanhaCommand : IRequest<Response<CriaCampanhaResult>>
    {
        public string Nome { get; set; }
        public Guid? TemplateId { get; set; }
        public string? ConteudoLivre { get; set; }
        public DateTime DataAgendamento { get; set; }
        public List<Guid> ContatoIds { get; set; }
        public Guid EmpresaId { get; set; }
    }
}
