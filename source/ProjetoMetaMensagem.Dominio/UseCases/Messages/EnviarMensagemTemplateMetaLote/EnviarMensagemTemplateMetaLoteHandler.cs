using ProjetoMetaMensagem.Dominio.Common;
using ProjetoMetaMensagem.Dominio.Entidades.Servico.Meta.Template.EnviarMensagemTemplateLote;
using ProjetoMetaMensagem.Dominio.Help.Error;
using ProjetoMetaMensagem.Dominio.Interfaces.Mediator;
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

        public EnviarMensagemTemplateMetaLoteHandler(IMetaService metaService)
        {
            _metaService = metaService;
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
                EnviarMensagemTemplateLoteRequisicao requisicao = new EnviarMensagemTemplateLoteRequisicao(command);

                // O serviço retorna o Dictionary<string, bool> contendo [Telefone -> Sucesso]
                var resultadoDisparos = await _metaService.EnviarTemplatesEmLoteAsync(requisicao);

                // 3. Montagem do objeto de resultado com as métricas do lote
                var resultadoLote = new EnviarMensagemTemplateMetaLoteResult
                {
                    RelatorioDisparos = resultadoDisparos
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
