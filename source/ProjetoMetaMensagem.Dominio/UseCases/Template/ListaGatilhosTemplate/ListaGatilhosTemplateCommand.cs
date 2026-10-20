using ProjetoMetaMensagem.Dominio.Common;
using ProjetoMetaMensagem.Dominio.Interfaces.Mediator;
using System;
using System.Collections.Generic;

namespace ProjetoMetaMensagem.Dominio.UseCases.Template.ListaGatilhosTemplate
{
    public class ListaGatilhosTemplateCommand : IRequest<Response<List<ListaGatilhosTemplateResult>>>
    {
        public Guid EmpresaId { get; set; }

        // Escopo do token (null = admin da plataforma): quando preenchido, vale ele e nao a rota.
        public Guid? EmpresaIdSolicitante { get; set; }
    }
}
