namespace ProjetoMetaMensagem.Dominio.Interfaces.Servicos
{
    public interface ITokenService
    {
        string GerarToken(string id, string email, string nome, string empresaId, string isAdmin);

        // A mesma decisao que vira a claim isAdminPlataforma no token, exposta para o login
        // poder contar isso a tela. Sem isso a tela so descobriria pelo erro da API.
        bool EhAdminDaPlataforma(string? email);
    }
}
