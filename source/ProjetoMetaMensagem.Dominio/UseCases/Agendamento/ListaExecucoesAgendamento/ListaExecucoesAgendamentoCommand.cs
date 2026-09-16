using ProjetoMetaMensagem.Dominio.Common;
using ProjetoMetaMensagem.Dominio.Interfaces.Mediator;
using System;
using System.Collections.Generic;

namespace ProjetoMetaMensagem.Dominio.UseCases.Agendamento.ListaExecucoesAgendamento
{
    public class ListaExecucoesAgendamentoCommand : IRequest<Response<List<ListaExecucoesAgendamentoResult>>>
    {
        public Guid AgendamentoId { get; set; }

        // Preenchido pelo controller a partir do JWT (null = administrador da plataforma).
        public Guid? EmpresaIdSolicitante { get; set; }
    }
}
