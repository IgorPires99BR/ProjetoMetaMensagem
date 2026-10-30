using System;

namespace ProjetoMetaMensagem.Dominio.UseCases.Empresa.ObtemProvedorCobranca
{
    public class ObtemProvedorCobrancaResult
    {
        public bool CobrancaPixAtiva { get; set; }
        public string? Provedor { get; set; }
        public string? NomeProvedor { get; set; }
        // True = cobranca em nome da empresa (conta dela); false = conta da Contact Solution.
        public bool ContaDaEmpresa { get; set; }
    }
}
