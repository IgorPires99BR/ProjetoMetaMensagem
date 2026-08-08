using ProjetoMetaMensagem.Dominio.Help.Error;
using Microsoft.Extensions.Logging;
using ProjetoMetaMensagem.Dominio.Common;
using ProjetoMetaMensagem.Dominio.Interfaces;
using ProjetoMetaMensagem.Dominio.Interfaces.Mediator;
using ProjetoMetaMensagem.Dominio.Interfaces.Servicos;
using System;
using System.Threading.Tasks;

namespace ProjetoMetaMensagem.Dominio.UseCases.Template.UploadMidiaTemplate
{
    public class UploadMidiaTemplateHandler : IRequestHandler<UploadMidiaTemplateCommand, Response<UploadMidiaTemplateResult>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMetaService _metaService;

        private readonly ILogger<UploadMidiaTemplateHandler> _logger;

        public UploadMidiaTemplateHandler(IUnitOfWork unitOfWork, IMetaService metaService, ILogger<UploadMidiaTemplateHandler> logger)
        {
            _unitOfWork = unitOfWork;
            _metaService = metaService;
            _logger = logger;
        }

        public async Task<Response<UploadMidiaTemplateResult>> Handle(UploadMidiaTemplateCommand command)
        {
            var response = new Response<UploadMidiaTemplateResult>();

            try
            {
                var appId = await _unitOfWork.Empresa.ObterAppIdMeta(command.EmpresaId);
                var token = await _unitOfWork.Empresa.ObterMetaAccessToken(command.EmpresaId);

                if (string.IsNullOrEmpty(appId))
                {
                    response.AddErro("Esta empresa não tem o App ID da Meta cadastrado. Preencha-o na tela de Empresas antes de enviar mídia de exemplo.");
                    return response;
                }

                var handle = await _metaService.UploadMidiaExemploAsync(appId, token, command.Arquivo, command.MimeType);

                response.AddValue(new UploadMidiaTemplateResult { Handle = handle });
            }
            catch (Exception ex)
            {
                response.AddErroServico(ex, _logger, nameof(UploadMidiaTemplateHandler));
            }

            return response;
        }
    }
}
