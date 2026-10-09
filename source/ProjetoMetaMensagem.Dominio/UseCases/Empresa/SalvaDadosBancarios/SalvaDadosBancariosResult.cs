using ProjetoMetaMensagem.Dominio.UseCases.Empresa.ObtemDadosBancarios;

namespace ProjetoMetaMensagem.Dominio.UseCases.Empresa.SalvaDadosBancarios
{
    // Devolve o estado gravado (mesmo formato da consulta) pra tela atualizar validade do
    // certificado e os indicadores de segredo sem precisar de um segundo GET.
    public class SalvaDadosBancariosResult
    {
        public ObtemDadosBancariosResult DadosBancarios { get; set; } = null!;

        // Resultado da conferencia com o Itau feita ao ativar a cobranca (credenciais e
        // webhook). Os dados ficam salvos mesmo com aviso: o Itau pode estar so fora do ar.
        public bool CredenciaisConferidas { get; set; }
        public bool WebhookCadastrado { get; set; }
        public string? AvisoIntegracao { get; set; }
    }
}
