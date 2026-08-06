using ProjetoMetaMensagem.Dominio.Common;
using ProjetoMetaMensagem.Dominio.Interfaces.Mediator;
using System;

namespace ProjetoMetaMensagem.Dominio.UseCases.Relatorio.ListaRelatorioMensagens
{
    public class ListaRelatorioMensagensCommand : IRequest<Response<ListaRelatorioMensagensResult>>
    {
        public Guid EmpresaId { get; set; }
        public DateTime? DataInicio { get; set; }
        public DateTime? DataFim { get; set; }
        public int Pagina { get; set; } = 0;
        public int TamanhoPagina { get; set; } = 50;
    }
}
