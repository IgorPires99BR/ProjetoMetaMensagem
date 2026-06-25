using ProjetoMetaMensagem.Dominio.Common;
using ProjetoMetaMensagem.Dominio.Interfaces.Mediator;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjetoMetaMensagem.Dominio.UseCases.Empresa.AtualizaWabaId
{
    public class AtualizaWabaIdCommand : IRequest<Response<AtualizaWabaIdResult>>
    {
        public AtualizaWabaIdCommand(Guid idEmpresa, string accessToken)
        {
            IdEmpresa = idEmpresa;
            AccessToken = accessToken;
        }

        public string AccessToken{ get; set; }
        public Guid IdEmpresa{ get; set; }
    }
}
