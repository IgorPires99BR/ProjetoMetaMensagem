using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjetoMetaMensagem.Dominio.UseCases.Contato.DeletaContato
{
    public class DeletaContatoResult
    {
        // true = o contato ja tinha historico e foi so inativado (some da lista, mas disparos e
        // conversas continuam no banco); false = foi apagado de vez.
        public bool Inativado { get; set; }
    }
}
