using Microsoft.Extensions.Options;
using ProjetoMetaMensagem.Dominio.Interfaces.Servicos;
using System;
using System.Security.Cryptography;
using System.Text;

namespace ProjetoMetaMensagem.Servico.Configuration
{
    public class CriptografiaService : ICriptografiaService
    {
        // Prefixo de versao no texto gravado: se um dia a chave ou o algoritmo mudar, da pra
        // saber com o que cada valor foi cifrado sem tentar adivinhar.
        private const string Versao = "v1:";
        private const int TamanhoNonce = 12;
        private const int TamanhoTag = 16;

        private readonly byte[]? _chave;

        public CriptografiaService(IOptions<CriptografiaConfiguration> options)
        {
            var chaveBase64 = options.Value.Chave;
            if (string.IsNullOrWhiteSpace(chaveBase64)) return;

            byte[] chave;
            try
            {
                chave = Convert.FromBase64String(chaveBase64);
            }
            catch (FormatException)
            {
                throw new InvalidOperationException("Criptografia:Chave precisa estar em base64.");
            }

            if (chave.Length != 32)
                throw new InvalidOperationException("Criptografia:Chave precisa ter 32 bytes (AES-256) depois de decodificada.");

            _chave = chave;
        }

        public bool Configurada => _chave != null;

        public string Criptografar(string textoPuro)
        {
            var chave = ObterChave();
            var dados = Encoding.UTF8.GetBytes(textoPuro);
            var nonce = RandomNumberGenerator.GetBytes(TamanhoNonce);
            var cifrado = new byte[dados.Length];
            var tag = new byte[TamanhoTag];

            using (var aes = new AesGcm(chave))
            {
                aes.Encrypt(nonce, dados, cifrado, tag);
            }

            var saida = new byte[TamanhoNonce + TamanhoTag + cifrado.Length];
            Buffer.BlockCopy(nonce, 0, saida, 0, TamanhoNonce);
            Buffer.BlockCopy(tag, 0, saida, TamanhoNonce, TamanhoTag);
            Buffer.BlockCopy(cifrado, 0, saida, TamanhoNonce + TamanhoTag, cifrado.Length);

            return Versao + Convert.ToBase64String(saida);
        }

        public string Descriptografar(string textoCriptografado)
        {
            var chave = ObterChave();
            if (!textoCriptografado.StartsWith(Versao, StringComparison.Ordinal))
                throw new InvalidOperationException("Valor criptografado em formato desconhecido.");

            var entrada = Convert.FromBase64String(textoCriptografado[Versao.Length..]);
            var nonce = entrada.AsSpan(0, TamanhoNonce);
            var tag = entrada.AsSpan(TamanhoNonce, TamanhoTag);
            var cifrado = entrada.AsSpan(TamanhoNonce + TamanhoTag);
            var dados = new byte[cifrado.Length];

            using (var aes = new AesGcm(chave))
            {
                aes.Decrypt(nonce, cifrado, tag, dados);
            }

            return Encoding.UTF8.GetString(dados);
        }

        private byte[] ObterChave() =>
            _chave ?? throw new InvalidOperationException("Criptografia:Chave não configurada.");
    }
}
