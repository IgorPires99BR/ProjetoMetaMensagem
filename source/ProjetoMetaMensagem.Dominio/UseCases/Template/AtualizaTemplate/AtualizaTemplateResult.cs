using System;

namespace ProjetoMetaMensagem.Dominio.UseCases.Template.AtualizaTemplate
{
    public class AtualizaTemplateResult
    {
        public AtualizaTemplateResult() { }

        public AtualizaTemplateResult(Entidades.Template template)
        {
            if (template != null)
            {
                Id = template.Id;
                IdEmpresa = template.EmpresaId;
                NomeTemplate = template.NomeTemplate;
                Conteudo = template.Conteudo;
                Categoria = template.Categoria;
                Idioma = template.Idioma;
                Status = template.Status;
                DataAtualizacao = template.DataAtualizacao;
                ComponentesJson = template.ComponentesJson;
            }
        }

        public Guid Id { get; set; }
        public Guid IdEmpresa { get; set; }
        public string NomeTemplate { get; set; }
        public string Conteudo { get; set; }
        public string Categoria { get; set; }
        public string Idioma { get; set; }
        public string Status { get; set; }
        public DateTime? DataAtualizacao { get; set; }
        public string? ComponentesJson { get; set; }
    }
}
