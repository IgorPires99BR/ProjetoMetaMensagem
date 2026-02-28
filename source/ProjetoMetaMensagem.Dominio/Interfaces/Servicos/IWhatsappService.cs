using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjetoMetaMensagem.Dominio.Interfaces.Servicos
{
    public interface IWhatsappService
    {
        Task<bool> EnviarMensagemAsync(string numero, string mensagem);
    }
}
