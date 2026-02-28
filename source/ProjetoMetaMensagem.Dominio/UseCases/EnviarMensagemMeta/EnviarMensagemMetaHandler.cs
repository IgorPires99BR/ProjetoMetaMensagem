using ProjetoMetaMensagem.Dominio.Common;
using ProjetoMetaMensagem.Dominio.Interfaces.Mediator;
using ProjetoMetaMensagem.Dominio.Interfaces.Servicos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjetoMetaMensagem.Dominio.UseCases.EnviarMensagemMeta
{
    public class EnviarMensagemMetaHandler : IRequestHandler<EnviarMensagemMetaCommand, Response<EnviarMensagemMetaResult>>
    {
        private readonly IWhatsappService _whatsappService;

        public EnviarMensagemMetaHandler(IWhatsappService whatsappService)
        {
            _whatsappService = whatsappService;
        }

        public async Task<Response<EnviarMensagemMetaResult>> Handle(EnviarMensagemMetaCommand request)
        {
            var response = new Response<EnviarMensagemMetaResult>();

            //var validator = new CriaClienteValidator();
            //var validateResult = validator.Validate(request);

            //if (!validateResult.IsValid)
            //{
            //    response.AddErros(validateResult.Errors.ToCustomValidationFailure());
            //    return response;
            //}

            // 2. Chamada ao serviço de integração com a Meta
            var sucesso = await _whatsappService.EnviarMensagemAsync(request.Celular, request.Template);

            if (sucesso == null)
            {
                response.AddErro("Erro ao Acessar a meta");
                return response;
            }

            return response;
        }
    }
}

