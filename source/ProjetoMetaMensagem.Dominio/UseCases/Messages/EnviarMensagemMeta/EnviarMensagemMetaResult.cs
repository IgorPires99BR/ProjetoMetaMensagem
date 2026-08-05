using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjetoMetaMensagem.Dominio.UseCases.Messages.EnviarMensagemMeta
{
    public class EnviarMensagemMetaResult
    {
        public bool Sucesso { get; set; }
        public string Mensagem { get; set; }
        public string WamidMeta { get; set; }
        public object Dados { get; set; }
    }
}
