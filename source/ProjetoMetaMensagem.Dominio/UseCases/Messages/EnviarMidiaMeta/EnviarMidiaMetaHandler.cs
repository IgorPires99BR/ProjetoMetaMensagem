using ProjetoMetaMensagem.Dominio.Help.Error;
using Microsoft.Extensions.Logging;
using ProjetoMetaMensagem.Dominio.Common;
using ProjetoMetaMensagem.Dominio.Entidades;
using ProjetoMetaMensagem.Dominio.Interfaces;
using ProjetoMetaMensagem.Dominio.Interfaces.Mediator;
using ProjetoMetaMensagem.Dominio.Interfaces.Servicos;
using System;
using System.Threading.Tasks;

namespace ProjetoMetaMensagem.Dominio.UseCases.Messages.EnviarMidiaMeta
{
    public class EnviarMidiaMetaHandler : IRequestHandler<EnviarMidiaMetaCommand, Response<EnviarMidiaMetaResult>>
    {
        private readonly IMetaService _whatsappService;
        private readonly IUnitOfWork _unitOfWork;

        private readonly ILogger<EnviarMidiaMetaHandler> _logger;

        public EnviarMidiaMetaHandler(IMetaService whatsappService, IUnitOfWork unitOfWork, ILogger<EnviarMidiaMetaHandler> logger)
        {
            _whatsappService = whatsappService;
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async Task<Response<EnviarMidiaMetaResult>> Handle(EnviarMidiaMetaCommand command)
        {
            var response = new Response<EnviarMidiaMetaResult>();

            var validator = new EnviarMidiaMetaValidator();
            var validateResult = validator.Validate(command);
            if (!validateResult.IsValid)
            {
                response.AddErros(validateResult.Errors.ToCustomValidationFailure());
                return response;
            }

            try
            {
                _unitOfWork.BeginTransaction();

                var token = await _unitOfWork.Empresa.ObterMetaAccessToken(command.EmpresaId);
                var phoneNumberId = await _unitOfWork.Empresa.ObterPhoneNumberId(command.EmpresaId);

                var (wamid, mediaId) = await _whatsappService.EnviarMidiaAsync(
                    command.Celular, command.Arquivo, command.MimeType, command.TipoMidia, token, phoneNumberId);

                var historico = new HistoricoDisparo
                {
                    EmpresaId = command.EmpresaId,
                    ContatoId = command.ContatoId,
                    TipoDisparo = "Livre",
                    Conteudo = $"[{command.TipoMidia}]",
                    WamidMeta = wamid,
                    TipoMidia = command.TipoMidia,
                    MidiaId = mediaId,
                    DataEnvio = DateTime.Now
                };

                await _unitOfWork.HistoricoDisparo.Incluir(historico);

                _unitOfWork.Commit();

                response.AddValue(new EnviarMidiaMetaResult
                {
                    Sucesso = true,
                    WamidMeta = historico.WamidMeta
                });
            }
            catch (Exception ex)
            {
                _unitOfWork.Rollback();
                response.AddErroServico(ex, _logger, nameof(EnviarMidiaMetaHandler));
            }

            return response;
        }
    }
}
