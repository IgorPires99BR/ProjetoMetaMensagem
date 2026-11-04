using ProjetoMetaMensagem.Dominio.Common;
using ProjetoMetaMensagem.Dominio.Interfaces.Mediator;
using System;

namespace ProjetoMetaMensagem.Dominio.UseCases.CobrancaCliente.EnviaRecorrenciaCobranca
{
    public class EnviaRecorrenciaCobrancaCommand : IRequest<Response<EnviaRecorrenciaCobrancaResult>>
    {
        public Guid CobrancaClienteId { get; set; }

        // Null = admin da plataforma (sem recorte), mesmo padrao das outras acoes da cobranca.
        public Guid? EmpresaIdSolicitante { get; set; }
    }
}
