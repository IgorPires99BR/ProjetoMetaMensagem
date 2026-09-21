using ProjetoMetaMensagem.Dominio.Common;
using ProjetoMetaMensagem.Dominio.Interfaces.Mediator;
using System;

namespace ProjetoMetaMensagem.Dominio.UseCases.Messages.EnviarMidiaMeta
{
    public class EnviarMidiaMetaCommand : IRequest<Response<EnviarMidiaMetaResult>>
    {
        public string Celular { get; set; }
        public Guid EmpresaId { get; set; }
        public Guid ContatoId { get; set; }
        public byte[] Arquivo { get; set; }
        public string MimeType { get; set; }
        public string TipoMidia { get; set; } // image, audio, document

        // Rota compartilhada entre a tela Disparador e o Chat manual -- vem do form-data da
        // requisicao e e validada/normalizada em OrigemDisparo.ResolverOuPadrao antes de gravar.
        public string? Origem { get; set; }
    }
}
