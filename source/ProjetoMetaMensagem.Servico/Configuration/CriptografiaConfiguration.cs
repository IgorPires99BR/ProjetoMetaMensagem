namespace ProjetoMetaMensagem.Servico.Configuration
{
    // Chave AES-256 em base64 (32 bytes). Sem default de proposito: e segredo, e trocar a chave
    // torna ilegivel tudo que ja foi gravado (as empresas teriam que recadastrar as credenciais
    // do banco). No Render: variavel Criptografia__Chave.
    public class CriptografiaConfiguration
    {
        public string? Chave { get; set; }
    }
}
