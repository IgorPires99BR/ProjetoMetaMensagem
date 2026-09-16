using ProjetoMetaMensagem.Dominio.Common;
using ProjetoMetaMensagem.Dominio.Interfaces.Mediator;
using System;

namespace ProjetoMetaMensagem.Dominio.UseCases.Agendamento.ObtemAgendamento
{
    public class ObtemAgendamentoCommand : IRequest<Response<ObtemAgendamentoResult>>
    {
        public Guid Id { get; set; }

        // Preenchido pelo controller a partir do JWT (null = administrador da plataforma).
        public Guid? EmpresaIdSolicitante { get; set; }
    }
}
