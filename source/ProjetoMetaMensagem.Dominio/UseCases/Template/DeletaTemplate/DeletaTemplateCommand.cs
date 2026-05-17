using ProjetoMetaMensagem.Dominio.Common;
using ProjetoMetaMensagem.Dominio.Interfaces.Mediator;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjetoMetaMensagem.Dominio.UseCases.Template.DeletaTemplate
{
    public class DeletaTemplateCommand : IRequest<Response<DeletaTemplateResult>>
    {
    }
}
