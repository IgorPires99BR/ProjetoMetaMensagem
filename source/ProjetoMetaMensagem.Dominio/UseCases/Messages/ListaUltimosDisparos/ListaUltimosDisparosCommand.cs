using ProjetoMetaMensagem.Dominio.Common;
using ProjetoMetaMensagem.Dominio.Interfaces.Mediator;
using System;
using System.Collections.Generic;

namespace ProjetoMetaMensagem.Dominio.UseCases.Messages.ListaUltimosDisparos
{
    public class ListaUltimosDisparosCommand : IRequest<Response<List<ListaUltimosDisparosResult>>>
    {
        public Guid EmpresaId { get; set; }

        // Vindo do token: quem nao e admin da plataforma so ve a propria empresa, seja qual for a rota.
        public Guid? EmpresaIdSolicitante { get; set; }
    }
}
