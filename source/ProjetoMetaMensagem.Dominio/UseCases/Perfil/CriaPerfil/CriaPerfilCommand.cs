using ProjetoMetaMensagem.Dominio.Common;
using ProjetoMetaMensagem.Dominio.Interfaces.Mediator;
using System;
using System.Collections.Generic;

namespace ProjetoMetaMensagem.Dominio.UseCases.Perfil.CriaPerfil
{
    public class CriaPerfilCommand : IRequest<Response<CriaPerfilResult>>
    {
        public string Nome { get; set; }
        public List<string> Telas { get; set; } = new();

        public Guid EmpresaId { get; set; }

        // Preenchido pelo controller a partir do JWT (null = administrador de plataforma).
        public Guid? EmpresaIdSolicitante { get; set; }
    }
}
