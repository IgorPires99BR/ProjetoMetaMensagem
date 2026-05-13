using ProjetoMetaMensagem.Dominio.Common;
using ProjetoMetaMensagem.Dominio.Interfaces.Mediator;
using ProjetoMetaMensagem.Dominio.Interfaces.Servicos;

namespace ProjetoMetaMensagem.Dominio.UseCases.Messages.EnviarMensagemTemplateMeta
{
    public class EnviarMensagemTemplateMetaHandler : IRequestHandler<EnviarMensagemTemplateMetaCommand, Response<EnviarMensagemTemplateMetaResult>>
    {
        private readonly IMetaService _whatsappService;

        public EnviarMensagemTemplateMetaHandler(IMetaService whatsappService)
        {
            _whatsappService = whatsappService;
        }

        public async Task<Response<EnviarMensagemTemplateMetaResult>> Handle(EnviarMensagemTemplateMetaCommand command)
        {
            var response = new Response<EnviarMensagemTemplateMetaResult>();

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
                var sucesso = await _whatsappService.EnviarTemplateAsync(new Entidades.Meta.Template.EnviarMensagemTemplate.EnviarMensagemTemplateRequisicao(command));

                if (sucesso == null)
                {
                    response.AddErro("Erro ao Acessar a meta");
                    return response;
                }

            }
            catch (Exception ex)
            {
                response.AddErro("Erro ao criar template na Meta:" + ex.Message);
            }

            return response;
        }
    }
}
