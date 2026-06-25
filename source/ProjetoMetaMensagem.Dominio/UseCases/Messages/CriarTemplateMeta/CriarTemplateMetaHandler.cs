using ProjetoMetaMensagem.Dominio.Common;
using ProjetoMetaMensagem.Dominio.Interfaces;
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
        private readonly IUnitOfWork _unitOfWork;

        public CriarTemplateMetaHandler(IMetaService whatsappService, IUnitOfWork unitOfWork)
        {
            _whatsappService = whatsappService;
            _unitOfWork = unitOfWork;
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
                var wabaId = await _unitOfWork.Empresa.ObterWabaId(command.IdEmpresa);
                var token = await _unitOfWork.Empresa.ObterMetaAccessToken(command.IdEmpresa);

                // 2. Chamada ao serviço de integração com a Meta
                var sucesso = await _whatsappService.CriarTemplateMetaAsync(new Entidades.Servico.Meta.Template.CreateTemplateRequisicao(command),wabaId,token);

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
