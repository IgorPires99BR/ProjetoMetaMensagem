using System;

namespace ProjetoMetaMensagem.Dominio.Entidades
{
    public class Pipeline
    {
        public Guid Id { get; set; }
        public Guid EmpresaId { get; set; }
        public string Nome { get; set; }
        public DateTime DataCriacao { get; set; }

        public Pipeline()
        {
            Id = Guid.NewGuid();
            DataCriacao = DateTime.Now;
        }
    }
}
