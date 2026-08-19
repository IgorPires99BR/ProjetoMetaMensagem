namespace ProjetoMetaMensagem.Dominio.UseCases.MensagemRecebida.AssumirConversa
{
    public class AssumirConversaResult
    {
        // false quando nao havia conversa ativa pra assumir (o flow ja tinha terminado, por
        // exemplo). A tela usa isso pra nao mostrar "assumido" sem nada ter acontecido.
        public bool Assumida { get; set; }
    }
}
