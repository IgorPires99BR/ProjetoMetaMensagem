using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjetoMetaMensagem.Dominio.UseCases.MensagemRecebida.ListaChatsAtivos
{
    public class ListaChatsAtivosResult
    {
        // O próprio result já entrega a lista diretamente
        public List<ChatAtivoObjeto> Chats { get; set; } = new List<ChatAtivoObjeto>();
    }

    public class ChatAtivoObjeto
    {
        public Guid ContatoId { get; set; }
        public string NomeContato { get; set; }
        public string Telefone { get; set; }
        public string UltimaMensagem { get; set; }
        public DateTime DataUltimaMensagem { get; set; }
        public int QuantidadeNaoLidas { get; set; }

        // true = ha um flow ativo tocando essa conversa (nenhum vendedor assumiu ainda)
        public bool RespondendoPorFlow { get; set; }
        // true = havia um flow ativo mas um vendedor assumiu manualmente (pode "devolver ao flow")
        public bool FlowAssumido { get; set; }

        // true = o bot desistiu (cliente respondeu fora do esperado duas vezes) e a conversa
        // esta esperando uma pessoa. A tela usa isto pra destacar quem precisa de atendimento.
        public bool AguardandoAtendente { get; set; }
    }
}
