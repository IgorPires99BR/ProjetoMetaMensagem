using ProjetoMetaMensagem.Dominio.Common;
using ProjetoMetaMensagem.Dominio.Interfaces.Mediator;
using System;

namespace ProjetoMetaMensagem.Dominio.UseCases.Empresa.IniciaConexaoMercadoPago
{
    public class IniciaConexaoMercadoPagoCommand : IRequest<Response<IniciaConexaoMercadoPagoResult>>
    {
        public Guid EmpresaId { get; set; }
    }
}
