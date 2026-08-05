using ProjetoMetaMensagem.Dominio.Common;
using ProjetoMetaMensagem.Dominio.Interfaces.Mediator;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjetoMetaMensagem.Dominio.UseCases.Template.CriaTemplate
{
    public class CriaTemplateCommand : IRequest<Response<CriaTemplateResult>>
    {
        public Guid IdEmpresa { get; set; }
        public string NomeTemplate { get; set; }  // Ex: "boas_vindas_clientes"
        public string Categoria { get; set; }     // Ex: "UTILITY", "MARKETING", "AUTHENTICATION"
        public string Idioma { get; set; }        // Ex: "pt_BR" (Pode vir default)
        public string Conteudo { get; set; }
        public List<CriaTemplateBotaoCommand>? Botoes { get; set; }
    }

    public class CriaTemplateBotaoCommand
    {
        public string Tipo { get; set; }  // QUICK_REPLY, URL, PHONE_NUMBER
        public string Texto { get; set; }
        public string? Url { get; set; }
        public string? NumeroTelefone { get; set; }
    }
}
