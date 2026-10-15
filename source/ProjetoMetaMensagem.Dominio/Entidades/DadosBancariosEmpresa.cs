using System;

namespace ProjetoMetaMensagem.Dominio.Entidades
{
    // Para onde vai a parte da empresa nas cobrancas aos clientes dela (ver BD/51, 53 e 54). O Pix
    // e gerado na conta Asaas da plataforma e o split repassa o valor menos a comissao para a
    // SUBCONTA Asaas da empresa (AsaasWalletId) -- o Asaas so faz split entre contas Asaas.
    //
    // As colunas da integracao direta com o Itau (agencia, conta, client id/secret,
    // certificado) e a chave Pix da Transfeera continuam na tabela, sem uso; ver BD/53 e BD/54.
    public class DadosBancariosEmpresa
    {
        public Guid EmpresaId { get; set; }

        // Subconta criada pela plataforma (POST /v3/accounts). WalletId e o destino do split;
        // a apiKey (criptografada) so serve para consultar a subconta depois.
        public string? AsaasContaId { get; set; }
        public string? AsaasWalletId { get; set; }
        public string? AsaasApiKeyCriptografada { get; set; }
        public DateTime? AsaasContaCriadaEm { get; set; }

        // Definida so pelo admin da plataforma. Nula = ainda nao combinada: a cobranca Pix nao
        // pode ser ativada, senao a plataforma repassaria 100% sem querer.
        public string? ComissaoTipo { get; set; }
        public decimal? ComissaoValor { get; set; }

        public bool CobrancaPixAtiva { get; set; }
        public DateTime DataCriacao { get; set; }
        public DateTime? DataAtualizacao { get; set; }

        public const string ComissaoPercentual = "PERCENTUAL";
        public const string ComissaoFixa = "FIXA";

        public bool ComissaoDefinida => ComissaoTipo != null && ComissaoValor.HasValue;
        public bool TemSubconta => !string.IsNullOrWhiteSpace(AsaasWalletId);

        // Conta Asaas propria da empresa, aberta fora da plataforma (a Sebrecon ja tinha uma e o
        // Asaas nao abre segunda conta no mesmo CNPJ). So o walletId e conhecido: sem AsaasContaId,
        // sem apiKey, e o GET /accounts da plataforma nao a enxerga.
        public bool CarteiraExterna => TemSubconta && string.IsNullOrWhiteSpace(AsaasContaId);
    }
}
