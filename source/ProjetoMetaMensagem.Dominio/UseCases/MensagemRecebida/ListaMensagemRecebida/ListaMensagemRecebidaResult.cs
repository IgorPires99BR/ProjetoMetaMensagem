using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjetoMetaMensagem.Dominio.UseCases.MensagemRecebida.ListaMensagemRecebida
{
    public class ListaMensagemRecebidaResult
    {
        public List<ItemMensagemChatDto> Mensagens { get; set; } = new();
    }

    public class ItemMensagemChatDto
    {
        public Guid Id { get; set; }

        /// <summary>
        /// Define a origem da mensagem para o layout do Angular ('user', 'bot' ou 'step')
        /// </summary>
        public string From { get; set; } = string.Empty;

        public string Text { get; set; } = string.Empty;

        /// <summary>
        /// Hora formatada em string (HH:mm) para exibição direta nos balões do chat
        /// </summary>
        public string Time { get; set; } = string.Empty;

        /// <summary>
        /// Identificador da mensagem na Meta (wamid), usado pra casar com o status de entrega. Nulo para mensagens recebidas.
        /// </summary>
        public string? Wamid { get; set; }

        /// <summary>
        /// Status de entrega (sent/delivered/read/failed) do disparo. Nulo para mensagens recebidas.
        /// </summary>
        public string? Status { get; set; }
    }
}
