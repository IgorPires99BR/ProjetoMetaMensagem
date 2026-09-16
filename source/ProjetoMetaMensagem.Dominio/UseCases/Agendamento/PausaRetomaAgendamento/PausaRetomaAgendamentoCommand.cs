using ProjetoMetaMensagem.Dominio.Common;
using ProjetoMetaMensagem.Dominio.Interfaces.Mediator;
using System;

namespace ProjetoMetaMensagem.Dominio.UseCases.Agendamento.PausaRetomaAgendamento
{
    public class PausaRetomaAgendamentoCommand : IRequest<Response<PausaRetomaAgendamentoResult>>
    {
        public Guid Id { get; set; }
        public bool Ativo { get; set; }

        // Preenchido pelo controller a partir do JWT (null = administrador da plataforma).
        public Guid? EmpresaIdSolicitante { get; set; }
    }
}
