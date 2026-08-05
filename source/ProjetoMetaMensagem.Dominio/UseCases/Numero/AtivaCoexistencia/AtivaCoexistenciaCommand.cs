using ProjetoMetaMensagem.Dominio.Common;
using ProjetoMetaMensagem.Dominio.Interfaces.Mediator;
using System;

namespace ProjetoMetaMensagem.Dominio.UseCases.Numero.AtivaCoexistencia
{
    public class AtivaCoexistenciaCommand : IRequest<Response<AtivaCoexistenciaResult>>
    {
        public AtivaCoexistenciaCommand(Guid numeroId, Guid idEmpresa)
        {
            NumeroId = numeroId;
            IdEmpresa = idEmpresa;
        }

        public Guid NumeroId { get; set; }
        public Guid IdEmpresa { get; set; }
    }
}
