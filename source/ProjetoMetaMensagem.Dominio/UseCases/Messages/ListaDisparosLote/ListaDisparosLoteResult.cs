using ProjetoMetaMensagem.Dominio.Entidades;
using System.Collections.Generic;

namespace ProjetoMetaMensagem.Dominio.UseCases.Messages.ListaDisparosLote
{
    public class ListaDisparosLoteResult
    {
        public List<DisparoLoteResumo> Lotes { get; set; } = new();
    }
}
