using ProjetoMetaMensagem.Dominio.Common;
using ProjetoMetaMensagem.Dominio.Interfaces.Mediator;
using System;

namespace ProjetoMetaMensagem.Dominio.UseCases.Numero.AtivaCoexistencia
{
    public class AtivaCoexistenciaCommand : IRequest<Response<AtivaCoexistenciaResult>>
    {
        public AtivaCoexistenciaCommand(Guid numeroId, Guid idEmpresa, string pin)
        {
            NumeroId = numeroId;
            IdEmpresa = idEmpresa;
            Pin = pin;
        }

        public Guid NumeroId { get; set; }
        public Guid IdEmpresa { get; set; }

        // PIN de verificação em 2 etapas do número na Meta (6 dígitos), exigido pelo endpoint /register
        public string Pin { get; set; }
    }
}
