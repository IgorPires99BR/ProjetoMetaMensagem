using ProjetoMetaMensagem.Dominio.Interfaces.Repositorios;
using System.Collections.Generic;

namespace ProjetoMetaMensagem.Dominio.UseCases.Relatorio.ListaRelatorioMensagens
{
    public class ListaRelatorioMensagensResult
    {
        public List<RelatorioMensagemDto> Mensagens { get; set; } = new();
    }
}
