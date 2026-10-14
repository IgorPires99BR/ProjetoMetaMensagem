using ProjetoMetaMensagem.Dominio.Common;
using ProjetoMetaMensagem.Dominio.Interfaces.Mediator;
using System;

namespace ProjetoMetaMensagem.Dominio.UseCases.Empresa.CriaSubcontaAsaas
{
    // Dados cadastrais que o Asaas exige para abrir a subconta da empresa (POST /v3/accounts).
    // Nao ficam gravados aqui: o Asaas e o dono do cadastro, aqui so guardamos ids.
    public class CriaSubcontaAsaasCommand : IRequest<Response<CriaSubcontaAsaasResult>>
    {
        public Guid EmpresaId { get; set; }
        public string? Nome { get; set; }
        public string? Email { get; set; }
        public string? CpfCnpj { get; set; }

        // So para CPF.
        public DateTime? DataNascimento { get; set; }

        // So para CNPJ: MEI, LIMITED, INDIVIDUAL ou ASSOCIATION.
        public string? TipoEmpresa { get; set; }

        public string? Celular { get; set; }
        public decimal? FaturamentoMensal { get; set; }
        public string? Endereco { get; set; }
        public string? Numero { get; set; }
        public string? Complemento { get; set; }
        public string? Bairro { get; set; }
        public string? Cep { get; set; }
    }
}
