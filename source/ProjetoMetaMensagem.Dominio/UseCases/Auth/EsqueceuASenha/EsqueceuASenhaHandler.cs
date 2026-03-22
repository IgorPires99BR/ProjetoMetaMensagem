using ProjetoMetaMensagem.Dominio.Common;
using ProjetoMetaMensagem.Dominio.Interfaces.Mediator;
using ProjetoMetaMensagem.Dominio.Interfaces.Servicos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjetoMetaMensagem.Dominio.UseCases.Auth.EsqueceuASenha
{
    public class EsqueceuASenhaHandler : IRequestHandler<EsqueceuASenhaCommand, Response<EsqueceuASenhaResult>>
    {
        private readonly IMetaService _whatsappService;

        public EsqueceuASenhaHandler(IMetaService whatsappService)
        {
            _whatsappService = whatsappService;
        }

        public async Task<Response<EsqueceuASenhaResult>> Handle(EsqueceuASenhaCommand request)
        {
            var response = new Response<EsqueceuASenhaResult>();

            return response;
        }
    }
}
