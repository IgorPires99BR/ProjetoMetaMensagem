using ProjetoMetaMensagem.Dominio.Common;
using ProjetoMetaMensagem.Dominio.Interfaces.Mediator;
using System;

namespace ProjetoMetaMensagem.Dominio.UseCases.Empresa.ObtemDadosBancarios
{
    public class ObtemDadosBancariosCommand : IRequest<Response<ObtemDadosBancariosResult>>
    {
        public ObtemDadosBancariosCommand(Guid empresaId)
        {
            EmpresaId = empresaId;
        }

        public Guid EmpresaId { get; set; }
    }
}
