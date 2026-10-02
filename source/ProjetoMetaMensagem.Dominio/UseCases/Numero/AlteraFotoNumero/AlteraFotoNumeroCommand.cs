using ProjetoMetaMensagem.Dominio.Common;
using ProjetoMetaMensagem.Dominio.Interfaces.Mediator;
using System;

namespace ProjetoMetaMensagem.Dominio.UseCases.Numero.AlteraFotoNumero
{
    public class AlteraFotoNumeroCommand : IRequest<Response<AlteraFotoNumeroResult>>
    {
        public Guid NumeroId { get; set; }

        // Escopo sempre vindo do token (nunca do corpo/rota)
        public Guid? EmpresaIdSolicitante { get; set; }

        public byte[] Arquivo { get; set; }
        public string MimeType { get; set; }
    }
}
