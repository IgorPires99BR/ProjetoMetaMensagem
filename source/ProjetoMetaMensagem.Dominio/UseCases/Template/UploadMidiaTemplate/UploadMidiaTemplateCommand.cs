using ProjetoMetaMensagem.Dominio.Common;
using ProjetoMetaMensagem.Dominio.Interfaces.Mediator;
using System;

namespace ProjetoMetaMensagem.Dominio.UseCases.Template.UploadMidiaTemplate
{
    public class UploadMidiaTemplateCommand : IRequest<Response<UploadMidiaTemplateResult>>
    {
        public Guid EmpresaId { get; set; }
        public byte[] Arquivo { get; set; } = Array.Empty<byte>();
        public string MimeType { get; set; } = string.Empty;
    }
}
