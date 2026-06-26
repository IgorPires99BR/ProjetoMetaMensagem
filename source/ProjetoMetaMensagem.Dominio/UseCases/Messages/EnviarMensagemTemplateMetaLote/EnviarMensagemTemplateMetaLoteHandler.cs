using Newtonsoft.Json;
using ProjetoMetaMensagem.Dominio.Common;
using ProjetoMetaMensagem.Dominio.Entidades;
using ProjetoMetaMensagem.Dominio.Entidades.Servico.Meta.Template.EnviarMensagemTemplateLote;
using ProjetoMetaMensagem.Dominio.Help.Error;
using ProjetoMetaMensagem.Dominio.Interfaces;
using ProjetoMetaMensagem.Dominio.Interfaces.Mediator;
using ProjetoMetaMensagem.Dominio.Interfaces.Servicos;
using ProjetoMetaMensagem.Dominio.UseCases.Numero.CriaNumero;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjetoMetaMensagem.Dominio.UseCases.Messages.EnviarMensagemTemplateMetaLote
{
    public class EnviarMensagemTemplateMetaLoteHandler : IRequestHandler<EnviarMensagemTemplateMetaLoteCommand, Response<EnviarMensagemTemplateMetaLoteResult>>
    {
        private readonly IMetaService _metaService;
        private readonly IUnitOfWork _unitOfWork;


        public EnviarMensagemTemplateMetaLoteHandler(IMetaService metaService, IUnitOfWork unitOfWork)
        {
            _metaService = metaService;
            _unitOfWork = unitOfWork;
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

                EnviarMensagemTemplateLoteRequisicao requisicao = new EnviarMensagemTemplateLoteRequisicao(command);

                // O serviço retorna o Dictionary<string, bool> contendo [Telefone -> Sucesso]
                var resultadoDisparos = await _metaService.EnviarTemplatesEmLoteAsync(requisicao, phoneNumberId,token);

	                foreach (var disparo in resultadoDisparos)
	                {
	                    var telefone = disparo.Key;
	                    var respostaMeta = disparo.Value;

	                    if (respostaMeta.Sucesso)
	                    {
	                        var historico = new HistoricoDisparo
	                        {
	                            EmpresaId = command.IdEmpresa,
	                            ContatoId = command.ContatoId,
	                            TemplateId = command.TemplateId,
	                            TipoDisparo = "Template",
	                            WamidMeta = respostaMeta.WamidMeta,
	                            Conteudo = JsonConvert.SerializeObject(new
	                            {
	                                command.ParametrosBody,
	                                command.ParametrosButton
	                            })
	                        };

	                        await _unitOfWork.HistoricoDisparo.Incluir(historico);
	                    }
	                    else
	                    {
	                        Debug.WriteLine($"[META ERROR LOTE] Falha no disparo para {telefone}: {respostaMeta.Erro}");
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
                response.AddErro($"Falha crítica ao processar disparo em lote: {ex.Message}");
            }


            return response;
        }
    }
}
