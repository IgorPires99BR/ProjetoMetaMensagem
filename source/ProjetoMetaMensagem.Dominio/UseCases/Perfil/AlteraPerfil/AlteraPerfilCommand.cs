using ProjetoMetaMensagem.Dominio.Common;
using ProjetoMetaMensagem.Dominio.Interfaces.Mediator;
using System;
using System.Collections.Generic;

namespace ProjetoMetaMensagem.Dominio.UseCases.Perfil.AlteraPerfil
{
    public class AlteraPerfilCommand : IRequest<Response<AlteraPerfilResult>>
    {
        public Guid Id { get; set; }
        public string Nome { get; set; }
        public List<string> Telas { get; set; } = new();

        public Guid? EmpresaIdSolicitante { get; set; }
    }
}
