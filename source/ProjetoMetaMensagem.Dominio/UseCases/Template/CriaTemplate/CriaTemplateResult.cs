using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjetoMetaMensagem.Dominio.UseCases.Template.CriaTemplate
{
    public class CriaTemplateResult
    {
        public CriaTemplateResult() { }

        public CriaTemplateResult(Entidades.Template template)
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
                DataCriacao = template.DataCriacao;
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
        public DateTime DataCriacao { get; set; }
        public string? ComponentesJson { get; set; }
    }
}
