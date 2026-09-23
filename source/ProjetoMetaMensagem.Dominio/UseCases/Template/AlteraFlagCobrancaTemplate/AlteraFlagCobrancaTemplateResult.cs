using System;

namespace ProjetoMetaMensagem.Dominio.UseCases.Template.AlteraFlagCobrancaTemplate
{
    public class AlteraFlagCobrancaTemplateResult
    {
        public Guid TemplateId { get; set; }
        public bool GeraCobranca { get; set; }
    }
}
