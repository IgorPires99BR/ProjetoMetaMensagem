using System;

namespace ProjetoMetaMensagem.Dominio.Entidades
{
    public class LeadPipeline
    {
        public Guid Id { get; set; }
        public Guid EmpresaId { get; set; }
        public Guid ContatoId { get; set; }
        public Guid PipelineEtapaId { get; set; }
        public decimal? Valor { get; set; }
        public string Observacao { get; set; }
        public DateTime DataEntrada { get; set; }
        public DateTime DataUltimaAlteracao { get; set; }
        public DateTime DataCriacao { get; set; }

        public LeadPipeline()
        {
            Id = Guid.NewGuid();
            DataCriacao = DateTime.Now;
            DataEntrada = DateTime.Now;
            DataUltimaAlteracao = DateTime.Now;
        }
    }
}
