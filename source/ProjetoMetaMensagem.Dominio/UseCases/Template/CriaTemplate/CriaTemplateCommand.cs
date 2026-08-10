using ProjetoMetaMensagem.Dominio.Common;
using ProjetoMetaMensagem.Dominio.Interfaces.Mediator;
using ProjetoMetaMensagem.Dominio.UseCases.Template.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjetoMetaMensagem.Dominio.UseCases.Template.CriaTemplate
{
    public class CriaTemplateCommand : IRequest<Response<CriaTemplateResult>>, ITemplateComponentesInput
    {
        public Guid IdEmpresa { get; set; }
        public string NomeTemplate { get; set; }  // Ex: "boas_vindas_clientes"
        public string Categoria { get; set; }     // Ex: "UTILITY", "MARKETING", "AUTHENTICATION"
        public string Idioma { get; set; }        // Ex: "pt_BR" (Pode vir default)
        public string Conteudo { get; set; }
        public List<CriaTemplateBotaoCommand>? Botoes { get; set; }

        // Cabeçalho opcional: "TEXT", "IMAGE", "VIDEO", "DOCUMENT" ou nulo/"NONE" (sem cabeçalho)
        public string? HeaderTipo { get; set; }
        public string? HeaderTexto { get; set; } // usado quando HeaderTipo == "TEXT"
        public string? HeaderExemploHandle { get; set; } // handle do upload, usado quando HeaderTipo é mídia

        // Rodapé opcional (máx. 60 caracteres na Meta)
        public string? FooterTexto { get; set; }

        // Valores de exemplo para cada {{n}} do corpo, na ordem — a Meta exige isso
        // quando o Conteudo tem variáveis
        public List<string>? ExemplosBody { get; set; }
    }

    public class CriaTemplateBotaoCommand
    {
        public string Tipo { get; set; }  // QUICK_REPLY, URL, PHONE_NUMBER, COPY_CODE
        public string Texto { get; set; }
        public string? Url { get; set; }
        public string? NumeroTelefone { get; set; }

        // Cupom/código de exemplo exigido pela Meta quando Tipo == COPY_CODE (a Meta define o
        // rótulo do botão sozinha — esse tipo não tem Texto livre como os demais)
        public string? CodigoExemplo { get; set; }
    }
}
