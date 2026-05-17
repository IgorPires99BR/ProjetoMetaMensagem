using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjetoMetaMensagem.Dominio.Entidades.Servico.Meta.Template.ObtemTemplateMeta
{
    public class ObtemTemplatesMetaResposta
    {
        public List<TemplateMetaDto> Templates { get; set; } = new();
    }

    public class TemplateMetaDto
    {
        public string Id { get; set; }
        public string Nome { get; set; }
        public string Status { get; set; }
        public string Categoria { get; set; }
        public string Idioma { get; set; }
        public string ConteudoCorpo { get; set; } // Texto limpo extraído do componente 'BODY'
    }
}
