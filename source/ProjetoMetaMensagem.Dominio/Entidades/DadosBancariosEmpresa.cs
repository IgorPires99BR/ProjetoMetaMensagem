using System;

namespace ProjetoMetaMensagem.Dominio.Entidades
{
    // Conta onde a empresa recebe o Pix das cobrancas aos clientes dela (ver BD/51).
    public class DadosBancariosEmpresa
    {
        public Guid EmpresaId { get; set; }
        public string Banco { get; set; } = CodigoItau;
        public string? Agencia { get; set; }
        public string? Conta { get; set; }
        public string? ContaDigito { get; set; }
        public string? TitularNome { get; set; }
        public string? TitularDocumento { get; set; }
        public string? TipoChavePix { get; set; }
        public string? ChavePix { get; set; }
        public string Ambiente { get; set; } = AmbienteSandbox;
        public string? ClientId { get; set; }
        public string? ClientSecretCriptografado { get; set; }
        public string? CertificadoPem { get; set; }
        public string? ChavePrivadaCriptografada { get; set; }
        public DateTime? CertificadoValidoAte { get; set; }
        public string? CertificadoTitular { get; set; }
        public bool CobrancaPixAtiva { get; set; }
        public DateTime DataCriacao { get; set; }
        public DateTime? DataAtualizacao { get; set; }

        public const string CodigoItau = "341";
        public const string AmbienteSandbox = "SANDBOX";
        public const string AmbienteProducao = "PRODUCAO";

        public static readonly string[] TiposChavePix = { "CNPJ", "CPF", "EMAIL", "TELEFONE", "ALEATORIA" };
    }
}
