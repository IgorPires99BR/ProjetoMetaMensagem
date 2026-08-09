using System.Collections.Generic;
using ProjetoMetaMensagem.Dominio.Enums;

namespace ProjetoMetaMensagem.Dominio.Interfaces.Servicos.Meta
{
    public class TemplateMetaDto
    {
        public string Id { get; set; }
        public string Nome { get; set; }
        public string Status { get; set; }
        public string Categoria { get; set; }
        public string Idioma { get; set; }
        public string ConteudoCorpo { get; set; }

        public List<ComponenteTemplateMetaDto> Componentes { get; set; } = new();
    }

    /// <summary>
    /// Representa um componente de template exatamente como retornado pela Meta (leitura).
    /// Deliberadamente separado de Entidades.TemplateComponenteDto (formato de persistência local)
    /// para nao acoplar o formato de leitura da API ao formato salvo no banco.
    /// </summary>
    public class ComponenteTemplateMetaDto
    {
        public TipoComponenteTemplate Tipo { get; set; }
        public TipoMidiaTemplate FormatMidia { get; set; } = TipoMidiaTemplate.None;
        public string? Texto { get; set; }
        public List<BotaoTemplateMetaDto>? Botoes { get; set; } = new();
    }

    public class BotaoTemplateMetaDto
    {
        public TipoBotaoTemplate Tipo { get; set; }
        public string Texto { get; set; }
        public string? Url { get; set; }
    }
}
