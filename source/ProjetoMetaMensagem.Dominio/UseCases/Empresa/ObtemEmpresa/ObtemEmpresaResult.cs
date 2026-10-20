using ProjetoMetaMensagem.Dominio.Entidades;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjetoMetaMensagem.Dominio.UseCases.Empresa.ObtemEmpresa
{
    public class ObtemEmpresaResult
    {
        // Como a empresa fala com a Meta. Conta propria = WABA diferente do da plataforma, que
        // so chega via Embedded Signup ("Conectar ao WhatsApp") ou credencial digitada a mao.
        public const string ConexaoContaPropria = "CONTA_PROPRIA";
        public const string ConexaoContactSolution = "CONTACT_SOLUTION";

        public ObtemEmpresaResult(Entidades.Empresa empresa, string? wabaIdDaPlataforma)
        {
            // Empresa nova recebe uma COPIA das credenciais da plataforma (CredenciaisMetaDaPlataforma),
            // entao WabaId preenchido nao basta: tem que ser diferente do da Contact Solution.
            TipoConexao = !string.IsNullOrWhiteSpace(empresa.WabaId)
                && !string.Equals(empresa.WabaId.Trim(), wabaIdDaPlataforma?.Trim(), StringComparison.Ordinal)
                    ? ConexaoContaPropria
                    : ConexaoContactSolution;
            AppIdMeta = empresa.AppIdMeta;

            Id = empresa.Id;
            Nome = empresa.Nome;
            Email = empresa.Email;
            Cnpj = empresa.Cnpj;
            // O token de producao da Meta NAO sai daqui em texto puro. Ele ia inteiro pro
            // navegador de qualquer pessoa da empresa que abrisse a tela (ou chamasse a API),
            // e com ele da pra disparar mensagem em nome do cliente por fora da plataforma,
            // sem passar por nenhum controle nosso. Sai so um resumo pra tela mostrar que
            // existe um token cadastrado e qual e o final dele.
            TemAccessToken = !string.IsNullOrWhiteSpace(empresa.MetaAccessToken);
            AccessToken = Mascarar(empresa.MetaAccessToken);
            PhoneNumberId = empresa.PhoneNumberId;
            PlanoId = empresa.PlanoId;
            WabaId = empresa.WabaId;
            Telefone = empresa.Telefone;
            // A coluna DataCriacao tem DEFAULT GETDATE() mas nao e NOT NULL: linha legada ou
            // insert que passe NULL explicito derrubava o GET da empresa inteiro.
            DataCriacao = empresa.DataCriacao ?? default;
        }

        public Guid Id { get; set; } 
        public string Nome { get; set; }
        public string? Email { get; set; }
        public string? Telefone { get; set; }
        public string? Cnpj { get; set; }
        public string? AccessToken { get; set; }
        public bool TemAccessToken { get; set; }
        public string? PhoneNumberId { get; set; }
        public string? WabaId { get; set; }
        public string? PlanoId { get; set; }
        // Nao e segredo (vai no SDK do navegador) e a tela precisa dele: sem ele, salvar a
        // empresa pela tela apagava o AppIdMeta, que o AlteraEmpresaHandler grava como veio.
        public string? AppIdMeta { get; set; }
        public string TipoConexao { get; set; }
        public DateTimeOffset DataCriacao { get; set; }

        // Prefixo reconhecido pelo AlteraEmpresaHandler: se o valor voltar assim, quer dizer
        // que o usuario nao digitou um token novo e o token atual deve ser preservado.
        public const string PrefixoMascara = "••••";

        private static string? Mascarar(string? token)
        {
            if (string.IsNullOrWhiteSpace(token)) return null;

            var finalDoToken = token.Length <= 4 ? token : token[^4..];
            return $"{PrefixoMascara}{finalDoToken}";
        }
    }
}
