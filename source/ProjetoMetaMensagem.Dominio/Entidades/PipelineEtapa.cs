using System;

namespace ProjetoMetaMensagem.Dominio.Entidades
{
    public class PipelineEtapa
    {
        public Guid Id { get; set; }
        public Guid PipelineId { get; set; }
        public string Nome { get; set; }
        public int Ordem { get; set; }
        public string Cor { get; set; }
        public bool DispararAoEntrar { get; set; }
        public Guid? TemplateIdAoEntrar { get; set; }
        public DateTime DataCriacao { get; set; }

        public PipelineEtapa()
        {
            Id = Guid.NewGuid();
            DataCriacao = DateTime.Now;
        }
    }
}
