using System;

namespace ProjetoMetaMensagem.Dominio.UseCases.Messages.ListaUltimosDisparos
{
    public class ListaUltimosDisparosResult
    {
        public Guid ContatoId { get; set; }
        public DateTime DataUltimoDisparo { get; set; }
    }
}
