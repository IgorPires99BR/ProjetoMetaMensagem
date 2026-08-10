using Newtonsoft.Json;
using ProjetoMetaMensagem.Dominio.Interfaces.Servicos.Meta;
using System.Collections.Generic;
using System.Linq;

namespace ProjetoMetaMensagem.Servico.MetaService.Wire
{
    // Payload de edição de template (POST /{template-id}) -- deliberadamente sem Name/Language:
    // a Meta não permite alterar nome nem idioma de um template já criado.
    public class EditTemplateRequest
    {
        public EditTemplateRequest(string categoria, List<ComponenteTemplateEnvio> componentes)
        {
            Category = categoria;

            if (componentes != null)
            {
                Components = componentes.Select(c => new TemplateComponent
                {
                    Type = c.Tipo,
                    Format = c.Formato,
                    Text = c.Texto,
                    Buttons = c.Botoes?.Select(b => new TemplateButtonDTO
                    {
                        Type = b.Tipo,
                        Text = b.Texto,
                        Url = b.Url,
                        PhoneNumber = b.NumeroTelefone
                    }).ToList(),
                    Example = (c.HeaderHandle != null || c.BodyTextExemplos != null)
                        ? new TemplateExampleRequest { HeaderHandle = c.HeaderHandle, BodyText = c.BodyTextExemplos }
                        : null
                }).ToList();
            }
        }

        [JsonProperty("category")]
        public string Category { get; set; }

        [JsonProperty("components")]
        public List<TemplateComponent> Components { get; set; }
    }
}
