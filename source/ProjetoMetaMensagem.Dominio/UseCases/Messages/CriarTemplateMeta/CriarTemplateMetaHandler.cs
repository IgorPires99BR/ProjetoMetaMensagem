using Microsoft.Extensions.Logging;
using ProjetoMetaMensagem.Dominio.Common;
using ProjetoMetaMensagem.Dominio.Help.Error;
using ProjetoMetaMensagem.Dominio.Interfaces;
using ProjetoMetaMensagem.Dominio.Interfaces.Mediator;
using ProjetoMetaMensagem.Dominio.Interfaces.Servicos;
using ProjetoMetaMensagem.Dominio.Interfaces.Servicos.Meta;
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
        private readonly ILogger<CriarTemplateMetaHandler> _logger;

        public CriarTemplateMetaHandler(IMetaService whatsappService, IUnitOfWork unitOfWork, ILogger<CriarTemplateMetaHandler> logger)
        {
            _whatsappService = whatsappService;
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async Task<Response<CriarTemplateMetaResult>> Handle(CriarTemplateMetaCommand command)
        {
            var response = new Response<CriarTemplateMetaResult>();

            var validator = new CriarTemplateMetaValidator();
            var validateResult = validator.Validate(command);

            if (!validateResult.IsValid)
            {
                response.AddErros(validateResult.Errors.ToCustomValidationFailure());
                return response;
            }

            try
            {
                var wabaId = await _unitOfWork.Empresa.ObterWabaId(command.IdEmpresa);
                var token = await _unitOfWork.Empresa.ObterMetaAccessToken(command.IdEmpresa);

                // 2. Chamada ao serviço de integração com a Meta
                var componentesMeta = command.Componentes?.Select(c => new ComponenteTemplateEnvio
                {
                    Tipo = c.Tipo,
                    Formato = c.Formato,
                    Texto = c.Texto,
                    Botoes = c.Botoes?.Select(b => new BotaoTemplateEnvio
                    {
                        Tipo = b.Tipo,
                        Texto = b.Texto,
                        Url = b.Url,
                        NumeroTelefone = b.NumeroTelefone
                    }).ToList()
                }).ToList();

                var sucesso = await _whatsappService.CriarTemplateMetaAsync(command.Nome, command.Idioma ?? "pt_BR", command.Categoria ?? "MARKETING", componentesMeta, wabaId, token);

                if (sucesso == null)
                {
                    response.AddErro("Erro ao Acessar a meta");
                    return response;
                }

            } catch (Exception ex) 
            { 
                response.AddErroServico(ex, _logger, nameof(CriarTemplateMetaHandler));
            }

            return response;
        }
    }
}
