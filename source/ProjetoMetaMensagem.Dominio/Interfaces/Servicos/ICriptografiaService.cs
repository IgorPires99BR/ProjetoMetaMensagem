namespace ProjetoMetaMensagem.Dominio.Interfaces.Servicos
{
    // Para credenciais de terceiros que precisamos LER de volta (ex: client secret e chave
    // privada da API Pix do banco da empresa) -- senha de usuario continua com BCrypt.
    public interface ICriptografiaService
    {
        // False quando a chave nao esta configurada: quem grava segredo deve recusar com uma
        // mensagem clara em vez de estourar excecao no meio do salvamento.
        bool Configurada { get; }

        string Criptografar(string textoPuro);
        string Descriptografar(string textoCriptografado);
    }
}
