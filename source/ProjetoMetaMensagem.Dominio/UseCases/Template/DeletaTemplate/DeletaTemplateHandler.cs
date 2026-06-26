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
        public async Task<Response<DeletaTemplateResult>> Handle(DeletaTemplateCommand request)
        {
            var response = new Response<DeletaTemplateResult>();

            try
            {
                throw new NotImplementedException();
            }
            catch (Exception ex)
            {
                response.AddErro($"Erro: {ex.Message}");
            }

            return response;
        }
    }
}
