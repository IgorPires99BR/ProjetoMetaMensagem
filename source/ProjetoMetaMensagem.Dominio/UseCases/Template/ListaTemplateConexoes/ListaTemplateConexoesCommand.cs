using ProjetoMetaMensagem.Dominio.Common;
using ProjetoMetaMensagem.Dominio.Interfaces.Mediator;
using System;
using System.Collections.Generic;

namespace ProjetoMetaMensagem.Dominio.UseCases.Template.ListaTemplateConexoes
{
    public class ListaTemplateConexoesCommand : IRequest<Response<List<ListaTemplateConexoesResult>>>
    {
        public ListaTemplateConexoesCommand(Guid empresaId)
        {
            EmpresaId = empresaId;
        }

        public Guid EmpresaId { get; set; }
    }
}
