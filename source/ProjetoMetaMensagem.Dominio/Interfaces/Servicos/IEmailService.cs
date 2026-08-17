using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjetoMetaMensagem.Dominio.Interfaces.Servicos
{
    public interface IEmailService
    {
        Task<bool> EnviarEmailAsync(string emailDestino, string novaSenha);

        // Primeiro contato com quem acabou de comprar: o texto de "recuperação de senha" não
        // serve aqui -- o cliente nunca pediu senha nova, ele comprou.
        Task<bool> EnviarBoasVindasAsync(string emailDestino, string nomeCliente, string senhaProvisoria);
    }
}
