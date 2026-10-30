using ProjetoMetaMensagem.Dominio.Common;
using ProjetoMetaMensagem.Dominio.Interfaces.Mediator;
using System;

namespace ProjetoMetaMensagem.Dominio.UseCases.Empresa.ConcluiConexaoMercadoPago
{
    // Retorno do Mercado Pago depois que a empresa autorizou o app: troca o code pelos tokens e
    // grava a conta como a de recebimento da empresa. Chega sem login (navegador vindo do Mercado
    // Pago): quem diz a empresa e o state, que so a propria API consegue gerar.
    public class ConcluiConexaoMercadoPagoCommand : IRequest<Response<ConcluiConexaoMercadoPagoResult>>
    {
        public string? Code { get; set; }
        public string? State { get; set; }
    }
}
