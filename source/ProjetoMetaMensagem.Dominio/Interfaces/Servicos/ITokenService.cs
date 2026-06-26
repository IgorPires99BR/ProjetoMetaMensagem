namespace ProjetoMetaMensagem.Dominio.Interfaces.Servicos
{
    public interface ITokenService
    {
        string GerarToken(string id, string email, string nome, string empresaId, string isAdmin);
    }
}
