using System.Collections.Generic;

namespace ProjetoMetaMensagem.Dominio.Interfaces.Servicos.Meta
{
    /// <summary>
    /// Vocabulario de entrada para a criacao de templates na Meta (escrita).
    /// Deliberadamente separado dos DTOs de leitura/persistencia de template.
    /// </summary>
    public class ComponenteTemplateEnvio
    {
        public string Tipo { get; set; } // HEADER, BODY, FOOTER, BUTTONS
        public string? Formato { get; set; } // TEXT, IMAGE, DOCUMENT, VIDEO (aplicavel a HEADER)
        public string? Texto { get; set; }
        public List<BotaoTemplateEnvio>? Botoes { get; set; }

        // Exigido pela Meta quando o componente tem variavel ({{n}} no BODY) ou é mídia no HEADER
        public List<string>? HeaderHandle { get; set; }
        public List<List<string>>? BodyTextExemplos { get; set; }
    }

    public class BotaoTemplateEnvio
    {
        public string Tipo { get; set; } // QUICK_REPLY, PHONE_NUMBER, URL, COPY_CODE
        public string Texto { get; set; }
        public string? Url { get; set; }
        public string? NumeroTelefone { get; set; }

        // Cupom de exemplo exigido pela Meta quando Tipo == COPY_CODE
        public string? CodigoExemplo { get; set; }
    }
}
