using System;

namespace ProjetoMetaMensagem.Dominio.Entidades
{
    public class ContatoTag
    {
        public Guid ContatoId { get; set; }
        public Guid TagId { get; set; }
        public DateTime DataCriacao { get; set; }
    }
}
