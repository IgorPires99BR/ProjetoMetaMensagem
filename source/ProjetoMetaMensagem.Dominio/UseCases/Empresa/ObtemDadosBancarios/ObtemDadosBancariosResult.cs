using ProjetoMetaMensagem.Dominio.Entidades;
using System;

namespace ProjetoMetaMensagem.Dominio.UseCases.Empresa.ObtemDadosBancarios
{
    public class ObtemDadosBancariosResult
    {
        // Empresa que ainda nao cadastrou nada recebe os padroes (Itau, sandbox), pra tela
        // abrir o formulario ja preenchido em vez de tratar "nao encontrado".
        public ObtemDadosBancariosResult(Guid empresaId, DadosBancariosEmpresa? dados)
        {
            dados ??= new DadosBancariosEmpresa { EmpresaId = empresaId };

            EmpresaId = empresaId;
            Banco = dados.Banco;
            Agencia = dados.Agencia;
            Conta = dados.Conta;
            ContaDigito = dados.ContaDigito;
            TitularNome = dados.TitularNome;
            TitularDocumento = dados.TitularDocumento;
            TipoChavePix = dados.TipoChavePix;
            ChavePix = dados.ChavePix;
            Ambiente = dados.Ambiente;
            ClientId = dados.ClientId;
            // Client secret e chave privada nunca voltam pro navegador, nem mascarados: com eles
            // da pra devolver Pix recebidos na conta do cliente por fora da plataforma.
            TemClientSecret = !string.IsNullOrWhiteSpace(dados.ClientSecretCriptografado);
            TemCertificado = !string.IsNullOrWhiteSpace(dados.CertificadoPem)
                && !string.IsNullOrWhiteSpace(dados.ChavePrivadaCriptografada);
            CertificadoValidoAte = dados.CertificadoValidoAte;
            CertificadoTitular = dados.CertificadoTitular;
            CobrancaPixAtiva = dados.CobrancaPixAtiva;
            DataAtualizacao = dados.DataAtualizacao ?? (dados.DataCriacao == default ? null : dados.DataCriacao);
        }

        public Guid EmpresaId { get; set; }
        public string Banco { get; set; }
        public string? Agencia { get; set; }
        public string? Conta { get; set; }
        public string? ContaDigito { get; set; }
        public string? TitularNome { get; set; }
        public string? TitularDocumento { get; set; }
        public string? TipoChavePix { get; set; }
        public string? ChavePix { get; set; }
        public string Ambiente { get; set; }
        public string? ClientId { get; set; }
        public bool TemClientSecret { get; set; }
        public bool TemCertificado { get; set; }
        public DateTime? CertificadoValidoAte { get; set; }
        public string? CertificadoTitular { get; set; }
        public bool CobrancaPixAtiva { get; set; }
        public DateTime? DataAtualizacao { get; set; }
    }
}
