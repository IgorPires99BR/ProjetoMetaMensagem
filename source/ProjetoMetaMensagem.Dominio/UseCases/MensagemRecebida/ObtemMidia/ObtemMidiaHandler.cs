using ProjetoMetaMensagem.Dominio.Help.Error;
using Microsoft.Extensions.Logging;
using ProjetoMetaMensagem.Dominio.Common;
using ProjetoMetaMensagem.Dominio.Interfaces;
using ProjetoMetaMensagem.Dominio.Interfaces.Mediator;
using ProjetoMetaMensagem.Dominio.Interfaces.Servicos;
using System;
using System.Threading.Tasks;

namespace ProjetoMetaMensagem.Dominio.UseCases.MensagemRecebida.ObtemMidia
{
    public class ObtemMidiaHandler : IRequestHandler<ObtemMidiaCommand, Response<ObtemMidiaResult>>
    {
        private readonly IMetaService _metaService;
        private readonly IUnitOfWork _unitOfWork;

        private readonly ILogger<ObtemMidiaHandler> _logger;

        public ObtemMidiaHandler(IMetaService metaService, IUnitOfWork unitOfWork, ILogger<ObtemMidiaHandler> logger)
        {
            _metaService = metaService;
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async Task<Response<ObtemMidiaResult>> Handle(ObtemMidiaCommand command)
        {
            var response = new Response<ObtemMidiaResult>();

            var validator = new ObtemMidiaValidator();
            var validateResult = validator.Validate(command);
            if (!validateResult.IsValid)
            {
                response.AddErros(validateResult.Errors.ToCustomValidationFailure());
                return response;
            }

            try
            {
                var token = await _unitOfWork.Empresa.ObterMetaAccessToken(command.EmpresaId);

                if (string.IsNullOrEmpty(token))
                {
                    response.AddErro("Empresa sem token de acesso à Meta configurado.");
                    return response;
                }

                var (bytes, mimeType) = await _metaService.BaixarMidiaAsync(command.MidiaId, token);

                response.AddValue(new ObtemMidiaResult
                {
                    Bytes = bytes,
                    MimeType = mimeType
                });
            }
            catch (Exception ex)
            {
                response.AddErroServico(ex, _logger, nameof(ObtemMidiaHandler));
            }

            return response;
        }
    }
}
