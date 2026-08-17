using System;
using System.Threading.Tasks;

namespace ProjetoMetaMensagem.Dominio.Interfaces.Servicos
{
    // Avisa o painel, em tempo real, sobre o que a plataforma acabou de ENVIAR.
    //
    // O webhook já transmite as mensagens recebidas, mas as respostas do bot não eram
    // transmitidas: o cliente recebia, e quem estava olhando o Chats não via nada até
    // recarregar a página.
    //
    // A interface fica no domínio e a implementação no WebAPI (onde vive o SignalR), para o
    // orquestrador de flow não depender de infraestrutura web.
    public interface INotificadorChat
    {
        Task NotificarMensagemEnviadaAsync(Guid empresaId, Guid contatoId, string conteudo, string? wamid);
    }
}
