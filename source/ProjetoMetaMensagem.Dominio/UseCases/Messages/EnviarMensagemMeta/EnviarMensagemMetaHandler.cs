using ProjetoMetaMensagem.Dominio.Help.Error;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using ProjetoMetaMensagem.Dominio.Common;
using ProjetoMetaMensagem.Dominio.Entidades;
using ProjetoMetaMensagem.Dominio.Interfaces;
using ProjetoMetaMensagem.Dominio.Interfaces.Mediator;
using ProjetoMetaMensagem.Dominio.Interfaces.Servicos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjetoMetaMensagem.Dominio.UseCases.Messages.EnviarMensagemMeta
{
    public class EnviarMensagemMetaHandler : IRequestHandler<EnviarMensagemMetaCommand, Response<EnviarMensagemMetaResult>>
    {
        private readonly IMetaService _whatsappService;
        private readonly IUnitOfWork _unitOfWork;

        private readonly ILogger<EnviarMensagemMetaHandler> _logger;

        public EnviarMensagemMetaHandler(IMetaService whatsappService, IUnitOfWork unitOfWork, ILogger<EnviarMensagemMetaHandler> logger)
        {
            _whatsappService = whatsappService;
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async Task<Response<EnviarMensagemMetaResult>> Handle(EnviarMensagemMetaCommand command)
        {
            var response = new Response<EnviarMensagemMetaResult>();

            var validator = new EnviarMensagemMetaValidator();
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

                var wamid = await _whatsappService.EnviarTextoLivreAsync(command.Celular, command.textoMensagem, token, phoneNumberId);

                // Registra historico do disparo
                var historico = new HistoricoDisparo
                {
                    EmpresaId = command.EmpresaId,
                    ContatoId = command.ContatoId,
                    TipoDisparo = "Livre",
                    Conteudo = command.textoMensagem,
                    WamidMeta = wamid,
                    DataEnvio = DateTime.Now
                };


                await _unitOfWork.HistoricoDisparo.Incluir(historico);

                // Vendedor respondendo manualmente pelo chat: se ha um flow ativo (e ainda nao
                // assumido) pra esse contato, pausa a automacao pra nao competir com a resposta
                // humana. Sem UsuarioIdSolicitante (token sem essa claim) nao ha o que registrar.
                if (command.UsuarioIdSolicitante.HasValue)
                {
                    var estadoAtivo = await _unitOfWork.ConversationState.ObterPorEmpresaEContato(command.EmpresaId, command.ContatoId);
                    if (estadoAtivo != null && estadoAtivo.AssumidoPorUsuarioId == null)
                    {
                        await _unitOfWork.ConversationState.AssumirManualmente(estadoAtivo.Id, command.UsuarioIdSolicitante.Value);
                    }
                }

                _unitOfWork.Commit();

                response.AddValue(new EnviarMensagemMetaResult
                {
                    Sucesso = true,
                    WamidMeta = historico.WamidMeta
                });
            }
            catch (Exception ex)
            {
                _unitOfWork.Rollback();
                response.AddErroServico(ex, _logger, nameof(EnviarMensagemMetaHandler));
            }

            return response;
        }
    }
}

