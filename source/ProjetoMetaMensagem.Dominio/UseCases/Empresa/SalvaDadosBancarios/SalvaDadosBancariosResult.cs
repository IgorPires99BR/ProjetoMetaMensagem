using ProjetoMetaMensagem.Dominio.UseCases.Empresa.ObtemDadosBancarios;

namespace ProjetoMetaMensagem.Dominio.UseCases.Empresa.SalvaDadosBancarios
{
    // Devolve o estado gravado (mesmo formato da consulta) pra tela atualizar sem precisar de
    // um segundo GET.
    public class SalvaDadosBancariosResult
    {
        public ObtemDadosBancariosResult DadosBancarios { get; set; } = null!;

        // Resultado da conferencia com o Asaas feita ao ativar a cobranca. Os dados ficam
        // salvos mesmo com aviso: o Asaas pode estar so fora do ar.
        public bool CredenciaisConferidas { get; set; }
        public string? AvisoIntegracao { get; set; }
    }
}
