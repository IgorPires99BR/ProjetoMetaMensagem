using Newtonsoft.Json;
using ProjetoMetaMensagem.Dominio.Common;
using ProjetoMetaMensagem.Dominio.Entidades;
using ProjetoMetaMensagem.Dominio.Interfaces;
using ProjetoMetaMensagem.Dominio.Interfaces.Mediator;
using ProjetoMetaMensagem.Dominio.Interfaces.Servicos;

namespace ProjetoMetaMensagem.Dominio.UseCases.Messages.EnviarMensagemTemplateMeta
{
    public class EnviarMensagemTemplateMetaHandler : IRequestHandler<EnviarMensagemTemplateMetaCommand, Response<EnviarMensagemTemplateMetaResult>>
    {
        private readonly IMetaService _whatsappService;
        private readonly IUnitOfWork _unitOfWork;

        public EnviarMensagemTemplateMetaHandler(IMetaService whatsappService, IUnitOfWork unitOfWork)
        {
            _whatsappService = whatsappService;
            _unitOfWork = unitOfWork;
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

                var phoneNumberId = await _unitOfWork.Empresa.ObterPhoneNumberId(command.IdEmpresa);
                var token = await _unitOfWork.Empresa.ObterMetaAccessToken(command.IdEmpresa);

                // 2. Chamada ao serviço de integração com a Meta
                var respostaMeta = await _whatsappService.EnviarTemplateAsync(new Entidades.Servico.Meta.Template.EnviarMensagemTemplate.EnviarMensagemTemplateRequisicao(command), phoneNumberId, token);

                if (respostaMeta == null)
                {
                    response.AddErro("Erro ao Acessar a meta");
                    return response;
                }

                if (!respostaMeta.Sucesso)
                {
                    response.AddErro($"Falha no disparo da Meta: {respostaMeta.Erro}");
                    return response;
                }

                // 3. Persistência no histórico em caso de sucesso
                //var historico = new HistoricoDisparo
                //{
                //    EmpresaId = command.EmpresaId,
                //    ContatoId = command.ContatoId,
                //    TemplateId = command.TemplateId,
                //    TipoDisparo = "Template",
                //    WamidMeta = respostaMeta.WamidMeta,
                //    // Serializa os parâmetros de body/button em JSON para auditoria na timeline
                //    Conteudo = JsonConvert.SerializeObject(new
                //    {
                //        command.ParametrosBody,
                //        command.ParametrosButton
                //    })
                //};

                //// Grava utilizando a propriedade do Unit of Work
                //await _unitOfWork.HistoricoDisparo.Incluir(historico);

                // Se o seu pipeline do Mediator não fizer o Commit de forma automática via Behavior/Middleware, 
                // descomente a linha abaixo para efetivar a transação:
                // await _unitOfWork.CommitAsync();

                // 4. Montagem do resultado positivo
                var resultado = new EnviarMensagemTemplateMetaResult
                {
                    Sucesso = true,
                    WamidMeta = respostaMeta.WamidMeta
                };

                response.AddValue(resultado);

            }
            catch (Exception ex)
            {
                response.AddErro("Erro ao criar template na Meta:" + ex.Message);
            }

            return response;
        }
    }
}
