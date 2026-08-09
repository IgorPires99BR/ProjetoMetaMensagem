using ProjetoMetaMensagem.Dominio.Enums;
using System.Collections.Generic;

namespace ProjetoMetaMensagem.Dominio.Entidades
{
    /// <summary>
    /// Formato persistido localmente (coluna JSON de Template.Componentes).
    /// Deliberadamente separado de Interfaces.Servicos.Meta.ComponenteTemplateMetaDto (formato de leitura da Meta).
    /// </summary>
    public class TemplateComponenteDto
    {
        /// <summary>
        /// Identifica a seção do template (Header, Body, Footer, Buttons)
        /// </summary>
        public TipoComponenteTemplate Tipo { get; set; }

        /// <summary>
        /// Define o formato de mídia esperado. Aplicável quando Tipo == TipoComponenteTemplate.Header
        /// (Pode ser Text, Image, Video, Document ou None)
        /// </summary>
        public TipoMidiaTemplate FormatMidia { get; set; } = TipoMidiaTemplate.None;

        /// <summary>
        /// Armazena o texto bruto da seção (Útil para ler as variáveis {{1}}, {{2}} do corpo ou texto do header/footer)
        /// </summary>
        public string? Texto { get; set; }

        /// <summary>
        /// Lista de ações e botões interativos vinculados. Aplicável quando Tipo == TipoComponenteTemplate.Buttons
        /// </summary>
        public List<TemplateBotaoDto>? Botoes { get; set; } = new();
    }

    public class TemplateBotaoDto
    {
        /// <summary>
        /// Tipo de comportamento do botão da Meta (QuickReply, Url, PhoneNumber)
        /// </summary>
        public TipoBotaoTemplate Tipo { get; set; }

        /// <summary>
        /// O texto visível que aparece impresso em cima do botão para o cliente clicar
        /// </summary>
        public string Texto { get; set; }

        /// <summary>
        /// O link/URL base cadastrado no botão. Aplicável quando Tipo == TipoBotaoTemplate.Url
        /// </summary>
        public string? Url { get; set; }
    }
}
