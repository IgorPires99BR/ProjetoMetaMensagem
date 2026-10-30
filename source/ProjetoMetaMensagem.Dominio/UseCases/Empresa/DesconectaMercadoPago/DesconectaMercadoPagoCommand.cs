using ProjetoMetaMensagem.Dominio.Common;
using ProjetoMetaMensagem.Dominio.Interfaces.Mediator;
using ProjetoMetaMensagem.Dominio.UseCases.Empresa.ObtemDadosBancarios;
using System;

namespace ProjetoMetaMensagem.Dominio.UseCases.Empresa.DesconectaMercadoPago
{
    // Esquece a conta Mercado Pago da empresa: as cobrancas novas voltam para a conta da Contact
    // Solution. Orders pendentes ja feitas na conta dela deixam de ter baixa automatica (sem token
    // nao da para consultar) -- a tela avisa antes.
    public class DesconectaMercadoPagoCommand : IRequest<Response<ObtemDadosBancariosResult>>
    {
        public Guid EmpresaId { get; set; }
    }
}
