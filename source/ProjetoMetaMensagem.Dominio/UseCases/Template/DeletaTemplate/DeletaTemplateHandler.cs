using ProjetoMetaMensagem.Dominio.Help.Error;
using Microsoft.Extensions.Logging;
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
        private readonly ILogger<DeletaTemplateHandler> _logger;

        public DeletaTemplateHandler(ILogger<DeletaTemplateHandler> logger)
        {
            _logger = logger;
        }

        public async Task<Response<DeletaTemplateResult>> Handle(DeletaTemplateCommand request)
        {
            var response = new Response<DeletaTemplateResult>();

            try
            {
                throw new NotImplementedException();
            }
            catch (Exception ex)
            {
                response.AddErro(TratamentoErro.Tratar(ex, _logger, nameof(DeletaTemplateHandler)));
            }

            return response;
        }
    }
}
