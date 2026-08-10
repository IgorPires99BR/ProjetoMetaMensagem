using System;

namespace ProjetoMetaMensagem.Dominio.UseCases.Template.DeletaTemplate
{
    public class DeletaTemplateResult
    {
        public DeletaTemplateResult() { }

        public DeletaTemplateResult(Entidades.Template template)
        {
            if (template != null)
            {
                Id = template.Id;
                NomeTemplate = template.NomeTemplate;
            }
        }

        public Guid Id { get; set; }
        public string NomeTemplate { get; set; }
    }
}
