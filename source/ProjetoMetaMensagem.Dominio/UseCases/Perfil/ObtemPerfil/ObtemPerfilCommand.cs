using ProjetoMetaMensagem.Dominio.Common;
using ProjetoMetaMensagem.Dominio.Interfaces.Mediator;
using System;
using System.Collections.Generic;

namespace ProjetoMetaMensagem.Dominio.UseCases.Perfil.ObtemPerfil
{
    public class ObtemPerfilCommand : IRequest<Response<List<ObtemPerfilResult>>>
    {
        public ObtemPerfilCommand(Guid? empresaIdSolicitante)
        {
            EmpresaIdSolicitante = empresaIdSolicitante;
        }

        public Guid? EmpresaIdSolicitante { get; set; }
    }
}
