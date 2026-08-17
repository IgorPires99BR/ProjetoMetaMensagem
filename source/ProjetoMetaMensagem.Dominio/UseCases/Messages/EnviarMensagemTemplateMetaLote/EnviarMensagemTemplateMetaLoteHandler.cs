using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using ProjetoMetaMensagem.Dominio.Common;
using ProjetoMetaMensagem.Dominio.Entidades;
using ProjetoMetaMensagem.Dominio.Help.Error;
using ProjetoMetaMensagem.Dominio.Interfaces;
using ProjetoMetaMensagem.Dominio.Interfaces.Mediator;
using ProjetoMetaMensagem.Dominio.Helpers.MensagemFormatter;
using ProjetoMetaMensagem.Dominio.Interfaces.Servicos;
using ProjetoMetaMensagem.Dominio.UseCases.Numero.CriaNumero;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjetoMetaMensagem.Dominio.UseCases.Messages.EnviarMensagemTemplateMetaLote
{
    public class EnviarMensagemTemplateMetaLoteHandler : IRequestHandler<EnviarMensagemTemplateMetaLoteCommand, Response<EnviarMensagemTemplateMetaLoteResult>>
    {
        private readonly IMetaService _metaService;
        private readonly IUnitOfWork _unitOfWork;


        private readonly ILogger<EnviarMensagemTemplateMetaLoteHandler> _logger;

        public EnviarMensagemTemplateMetaLoteHandler(IMetaService metaService, IUnitOfWork unitOfWork, ILogger<EnviarMensagemTemplateMetaLoteHandler> logger)
        {
            _metaService = metaService;
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async Task<Response<EnviarMensagemTemplateMetaLoteResult>> Handle(EnviarMensagemTemplateMetaLoteCommand command)
        {

            var response = new Response<EnviarMensagemTemplateMetaLoteResult>();

            var validator = new EnviarMensagemTemplateMetaLoteValidator();
            var validateResult = validator.Validate(command);

            if (!validateResult.IsValid)
            {
                response.AddErros(validateResult.Errors.ToCustomValidationFailure());
                return response;
            }

            try 
            {
                var phoneNumberId = await _unitOfWork.Empresa.ObterPhoneNumberId(command.IdEmpresa);
                var token = await _unitOfWork.Empresa.ObterMetaAccessToken(command.IdEmpresa);

                // O serviço retorna o Dictionary<string, ResultadoEnvioTemplate> contendo [Telefone -> Resultado]
                var resultadoDisparos = await _metaService.EnviarTemplatesEmLoteAsync(command, phoneNumberId, token);

                // Uma consulta so; o texto muda por destinatario quando o disparo e personalizado.
                var templateEnviado = command.TemplateId.HasValue
                    ? await _unitOfWork.Template.ObterPorIdEEmpresa(command.TemplateId.Value, command.IdEmpresa)
                    : null;

                foreach (var disparo in resultadoDisparos)
                {
                    var telefone = disparo.Key;
                    var respostaMeta = disparo.Value;

                    if (respostaMeta.Sucesso)
                    {
                        var historico = new HistoricoDisparo
                        {
                            EmpresaId = command.IdEmpresa,
                            // ✅ Recupera o ID específico e correto que mapeamos para este número de telefone
                            ContatoId = Guid.Parse(respostaMeta.ContatoId),
                            TemplateId = command.TemplateId,
                            TipoDisparo = "Template",
                            WamidMeta = respostaMeta.WamidMeta,
                            // Texto legivel pro usuario, com os valores que ESTE contato
                            // recebeu; o JSON da Meta vai na coluna de auditoria, sem poluir
                            // o que aparece no chat e no relatorio.
                            Conteudo = TemplateTextoHelper.MontarTextoEnviado(
                                templateEnviado?.Conteudo,
                                command.NomeTemplate,
                                command.ParametrosBodyDe(telefone)),
                            PayloadEnvio = respostaMeta.JsonEnviado
                        };

                        await _unitOfWork.HistoricoDisparo.Incluir(historico);
                    }
                }

                // 3. Montagem do objeto de resultado mantendo o dicionário original [Telefone -> bool] para a View do CRM
                var resultadoLote = new EnviarMensagemTemplateMetaLoteResult
                {
                    RelatorioDisparos = resultadoDisparos.ToDictionary(x => x.Key, x => x.Value.Sucesso),
                    RelatorioErros = resultadoDisparos
                        .Where(x => !x.Value.Sucesso && !string.IsNullOrEmpty(x.Value.Erro))
                        .ToDictionary(x => x.Key, x => x.Value.Erro)
                };

                // Atribui o resultado de sucesso ao envelope da Response
                response.AddValue(resultadoLote);


            }
            catch (Exception ex) 
            {
                response.AddErroServico(ex, _logger, nameof(EnviarMensagemTemplateMetaLoteHandler));
            }


            return response;
        }
    }
}
