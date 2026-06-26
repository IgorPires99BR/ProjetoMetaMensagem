using ProjetoMetaMensagem.Dominio.Common;
using ProjetoMetaMensagem.Dominio.Interfaces.Mediator;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjetoMetaMensagem.Dominio.UseCases.Messages.EnviarMensagemTemplateMetaLote
{
    public class EnviarMensagemTemplateMetaLoteCommand : IRequest<Response<EnviarMensagemTemplateMetaLoteResult>>
    {
        public Guid IdEmpresa { get; set; }
        public List<string> Telefones { get; set; } = new List<string>();
        public string NomeTemplate { get; set; }
        public string Idioma { get; set; } = "pt_BR";
        public Guid EmpresaId { get; set; }
        public Guid ContatoId { get; set; }
        public Guid? TemplateId { get; set; }
        public List<string> ParametrosBody { get; set; } = new List<string>();
        public List<string> ParametrosButton { get; set; } = new List<string>();
    }
}
