using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjetoMetaMensagem.Dominio.Enums
{
    public enum TipoBotaoTemplate
    {
        QuickReply = 0,
        Url = 1,
        PhoneNumber = 2,

        // Adicionado no final de propósito: os 3 valores acima já estão persistidos como número
        // em produção (ComponentesJson) -- nunca reordenar os existentes.
        CopyCode = 3
    }
}
