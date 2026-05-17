using ProjetoMetaMensagem.Dominio.Common;
using ProjetoMetaMensagem.Dominio.Interfaces.Mediator;
using ProjetoMetaMensagem.Dominio.Interfaces.Servicos;
using ProjetoMetaMensagem.Dominio.UseCases.Messages.EnviarMensagemMeta;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjetoMetaMensagem.Dominio.UseCases.Messages.CriarTemplateMeta
{
    public class CriarTemplateMetaHandler : IRequestHandler<CriarTemplateMetaCommand, Response<CriarTemplateMetaResult>>
    {
        private readonly IMetaService _whatsappService;

        public CriarTemplateMetaHandler(IMetaService whatsappService)
        {
            _whatsappService = whatsappService;
        }

        public async Task<Response<CriarTemplateMetaResult>> Handle(CriarTemplateMetaCommand command)
        {
            var response = new Response<CriarTemplateMetaResult>();

            //var validator = new CriaClienteValidator();
            //var validateResult = validator.Validate(request);

            //if (!validateResult.IsValid)
            //{
            //    response.AddErros(validateResult.Errors.ToCustomValidationFailure());
            //    return response;
            //}

            try
            {
                // 2. Chamada ao serviço de integração com a Meta
                var sucesso = await _whatsappService.CriarTemplateMetaAsync(new Entidades.Servico.Meta.Template.CreateTemplateRequisicao(command));

                if (sucesso == null)
                {
                    response.AddErro("Erro ao Acessar a meta");
                    return response;
                }

            } catch (Exception ex) 
            { 
                response.AddErro("Erro ao criar template na Meta:" + ex.Message);
            }

            return response;
        }
    }
}
