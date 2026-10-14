using ProjetoMetaMensagem.Dominio.UseCases.Empresa.ObtemDadosBancarios;

namespace ProjetoMetaMensagem.Dominio.UseCases.Empresa.VinculaSubcontaAsaas
{
    public class VinculaSubcontaAsaasResult
    {
        public ObtemDadosBancariosResult DadosBancarios { get; set; } = null!;
        public string NomeSubconta { get; set; } = string.Empty;
    }
}
