using ProjetoMetaMensagem.Dominio.Common;
using ProjetoMetaMensagem.Dominio.Interfaces.Mediator;
using System;

namespace ProjetoMetaMensagem.Dominio.UseCases.Contato.ReativaContato
{
    public class ReativaContatoCommand : IRequest<Response<ReativaContatoResult>>
    {
        public Guid Id { get; set; }

        // Preenchido pelo controller a partir do JWT (null = administrador, enxerga tudo).
        public Guid? EmpresaIdSolicitante { get; set; }
    }
}
