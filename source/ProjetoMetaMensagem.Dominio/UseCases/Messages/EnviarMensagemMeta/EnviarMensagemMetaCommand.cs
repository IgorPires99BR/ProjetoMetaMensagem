using ProjetoMetaMensagem.Dominio.Common;
using ProjetoMetaMensagem.Dominio.Interfaces.Mediator;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjetoMetaMensagem.Dominio.UseCases.Messages.EnviarMensagemMeta
{
    public class EnviarMensagemMetaCommand : IRequest<Response<EnviarMensagemMetaResult>>
    {
        public string Celular { get; set; }
        public string Template { get; set; }
        public string textoMensagem { get; set; }
        public Guid EmpresaId { get; set; }
        public Guid ContatoId { get; set; }

        // Preenchido pelo controller a partir do JWT (nunca do corpo). Usado para marcar a
        // conversa como assumida manualmente quando ha um flow ativo tocando pra esse contato.
        public Guid? UsuarioIdSolicitante { get; set; }

        // Rota compartilhada entre a tela Disparador e o Chat manual -- vem do corpo da
        // requisicao e e validada/normalizada em OrigemDisparo.ResolverOuPadrao antes de gravar.
        public string? Origem { get; set; }
    }
}
