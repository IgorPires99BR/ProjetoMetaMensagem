using ProjetoMetaMensagem.Dominio.Common;
using ProjetoMetaMensagem.Dominio.Interfaces.Mediator;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjetoMetaMensagem.Dominio.UseCases.Empresa.ObtemEmpresa
{
    public class ObtemEmpresaCommand : IRequest<Response<List<ObtemEmpresaResult>>>
    {
        // Preenchidos pelo controller a partir das claims do JWT, nunca vindos do cliente.
        // Sem isso o endpoint devolvia TODAS as empresas (com o MetaAccessToken de cada uma)
        // pra qualquer usuario autenticado, de qualquer tenant.
        public Guid? EmpresaIdSolicitante { get; set; }
        public bool SolicitanteEhAdmin { get; set; }
    }
}
