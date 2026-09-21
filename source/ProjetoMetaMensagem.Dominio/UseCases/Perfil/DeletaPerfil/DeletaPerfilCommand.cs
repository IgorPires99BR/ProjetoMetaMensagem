using ProjetoMetaMensagem.Dominio.Common;
using ProjetoMetaMensagem.Dominio.Interfaces.Mediator;
using System;

namespace ProjetoMetaMensagem.Dominio.UseCases.Perfil.DeletaPerfil
{
    public class DeletaPerfilCommand : IRequest<Response<DeletaPerfilResult>>
    {
        public Guid Id { get; set; }
        public Guid? EmpresaIdSolicitante { get; set; }
    }
}
