using ProjetoMetaMensagem.Dominio.Common;
using ProjetoMetaMensagem.Dominio.Interfaces.Mediator;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace ProjetoMetaMensagem.Dominio.UseCases.Messages.CriarTemplateMeta
{
    public class CriarTemplateMetaCommand : IRequest<Response<CriarTemplateMetaResult>>
    {
        public string Nome { get; set; }
        public string Idioma { get; set; } = "pt_BR";
        public string Categoria { get; set; } = "MARKETING";
        public List<ComponenteTemplateCommand> Componentes { get; set; }
    }

    public class ComponenteTemplateCommand
    {
        [JsonPropertyName("tipo")] // Mapeia a entrada do JSON 'tipo' para a propriedade 'Tipo'
        public string Tipo { get; set; } // BODY, HEADER, etc.
        public string? Formato { get; set; } // TEXT, IMAGE (Opcional)
        [JsonPropertyName("texto")] // Mapeia a entrada do JSON 'tipo' para a propriedade 'Tipo'
        public string? Texto { get; set; } // Opcional (ex: botões não têm texto aqui)
        public List<BotaoTemplateCommand>? Botoes { get; set; } // Opcional
    }

    public class BotaoTemplateCommand
    {
        public string Tipo { get; set; } // QUICK_REPLY, URL, etc.
        public string Texto { get; set; }
        public string? Url { get; set; } // Opcional
        public string? NumeroTelefone { get; set; } // Opcional
    }
}
