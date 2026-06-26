using ProjetoMetaMensagem.Dominio.Common;
using ProjetoMetaMensagem.Dominio.Interfaces.Mediator;

namespace ProjetoMetaMensagem.Dominio.UseCases.Messages.EnviarMensagemTemplateMeta
{
    public class EnviarMensagemTemplateMetaCommand : IRequest<Response<EnviarMensagemTemplateMetaResult>>
    {
        public Guid IdEmpresa { get; set; }
        public string Telefone { get; set; }
        public string NomeTemplate { get; set; }
        public string Idioma { get; set; } = "pt_BR";
        public Guid ContatoId { get; set; }
        public Guid? TemplateId { get; set; }
        public Guid EmpresaId { get; set; }
        public List<string> ParametrosBody { get; set; } = new List<string>();
        public List<string> ParametrosButton { get; set; } = new List<string>();
    }
}
