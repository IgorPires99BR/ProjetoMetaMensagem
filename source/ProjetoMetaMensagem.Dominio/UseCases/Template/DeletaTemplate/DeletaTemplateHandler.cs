using ProjetoMetaMensagem.Dominio.Common;
using ProjetoMetaMensagem.Dominio.Interfaces.Mediator;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjetoMetaMensagem.Dominio.UseCases.Template.DeletaTemplate
{
    public class DeletaTemplateHandler : IRequestHandler<DeletaTemplateCommand, Response<DeletaTemplateResult>>
    {
        public Task<Response<DeletaTemplateResult>> Handle(DeletaTemplateCommand request)
        {
            throw new NotImplementedException();
        }
    }
}
