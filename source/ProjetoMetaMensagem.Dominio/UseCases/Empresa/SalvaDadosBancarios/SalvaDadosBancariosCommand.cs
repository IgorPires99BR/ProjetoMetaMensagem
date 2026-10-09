using ProjetoMetaMensagem.Dominio.Common;
using ProjetoMetaMensagem.Dominio.Interfaces.Mediator;
using System;

namespace ProjetoMetaMensagem.Dominio.UseCases.Empresa.SalvaDadosBancarios
{
    public class SalvaDadosBancariosCommand : IRequest<Response<SalvaDadosBancariosResult>>
    {
        public Guid EmpresaId { get; set; }
        public string Banco { get; set; } = Entidades.DadosBancariosEmpresa.CodigoItau;
        public string? Agencia { get; set; }
        public string? Conta { get; set; }
        public string? ContaDigito { get; set; }
        public string? TitularNome { get; set; }
        public string? TitularDocumento { get; set; }
        public string? TipoChavePix { get; set; }
        public string? ChavePix { get; set; }
        public string Ambiente { get; set; } = Entidades.DadosBancariosEmpresa.AmbienteSandbox;
        public string? ClientId { get; set; }
        public bool CobrancaPixAtiva { get; set; }

        // Segredos: em branco mantem o que ja esta gravado (a tela nunca recebe o valor atual
        // de volta, entao nao tem como reenviar). Certificado e chave privada vem sempre juntos,
        // em PEM, e so sao aceitos se a chave for mesmo a do certificado.
        public string? ClientSecret { get; set; }
        public string? CertificadoPem { get; set; }
        public string? ChavePrivadaPem { get; set; }
    }
}
