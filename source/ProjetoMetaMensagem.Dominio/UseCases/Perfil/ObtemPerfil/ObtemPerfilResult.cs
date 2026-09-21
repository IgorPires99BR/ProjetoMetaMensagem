using System;
using System.Collections.Generic;

namespace ProjetoMetaMensagem.Dominio.UseCases.Perfil.ObtemPerfil
{
    public class ObtemPerfilResult
    {
        public ObtemPerfilResult()
        {
        }

        public ObtemPerfilResult(Entidades.Perfil perfil)
        {
            Id = perfil.Id;
            EmpresaId = perfil.EmpresaId;
            Nome = perfil.Nome;
            Telas = perfil.Telas;
        }

        public Guid Id { get; set; }
        public Guid EmpresaId { get; set; }
        public string Nome { get; set; }
        public List<string> Telas { get; set; } = new();
    }
}
