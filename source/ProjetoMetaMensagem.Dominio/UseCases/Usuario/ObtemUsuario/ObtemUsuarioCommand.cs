using ProjetoMetaMensagem.Dominio.Common;
using ProjetoMetaMensagem.Dominio.Interfaces.Mediator;
using System;
using System.Collections.Generic;

namespace ProjetoMetaMensagem.Dominio.UseCases.Usuario.ObtemUsuario
{
    public class ObtemUsuarioCommand : IRequest<Response<List<ObtemUsuarioResult>>>
    {
        // Nome historico do parametro (era "idUsuario", mas sempre guardou um EmpresaId --
        // ver o antigo "ObterPorId(Guid idEmpresa)" no controller). Mantido pra nao quebrar
        // quem ja chama isto, so ficou nullable: null = todas as empresas (so a conta de
        // plataforma pode pedir isso -- o controller garante).
        public ObtemUsuarioCommand(Guid? idEmpresa)
        {
            IdEmpresa = idEmpresa;
        }

        public Guid? IdEmpresa { get; set; }
    }
}
