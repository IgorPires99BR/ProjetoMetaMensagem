using Microsoft.Extensions.Logging;
using ProjetoMetaMensagem.Dominio.Common;
using ProjetoMetaMensagem.Dominio.Entidades;
using ProjetoMetaMensagem.Dominio.Enums;
using ProjetoMetaMensagem.Dominio.Help.Error;
using ProjetoMetaMensagem.Dominio.Interfaces;
using ProjetoMetaMensagem.Dominio.Interfaces.Mediator;
using ProjetoMetaMensagem.Dominio.Interfaces.Servicos;
using ProjetoMetaMensagem.Dominio.Interfaces.Servicos.Meta;
using ProjetoMetaMensagem.Dominio.UseCases.Template.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace ProjetoMetaMensagem.Dominio.UseCases.Template.CriaTemplate
{
    public class CriaTemplateHandler : IRequestHandler<CriaTemplateCommand, Response<CriaTemplateResult>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMetaService _metaService;

        private readonly ILogger<CriaTemplateHandler> _logger;

        public CriaTemplateHandler(IUnitOfWork unitOfWork, IMetaService metaService, ILogger<CriaTemplateHandler> logger)
        {
            _unitOfWork = unitOfWork;
            _metaService = metaService;
            _logger = logger;
        }

        public async Task<Response<CriaTemplateResult>> Handle(CriaTemplateCommand command)
        {
            var response = new Response<CriaTemplateResult>();

            // 1. Validação do Comando (Entrada do Request do Mediator)
            var validator = new CriaTemplateValidator();
            var validateResult = validator.Validate(command);

            if (!validateResult.IsValid)
            {
                response.AddErros(validateResult.Errors.ToCustomValidationFailure());
                return response;
            }

            var criadoNaMeta = false;

            try
            {
                _unitOfWork.BeginTransaction();
                // 2. Monta o payload estruturado que o 'CriarTemplateMetaAsync' do seu MetaService espera
                var componentesMeta = TemplateComponentesBuilder.MontarComponentesEnvio(command);

                var wabaId = await _unitOfWork.Empresa.ObterWabaId(command.IdEmpresa);
                var token = await _unitOfWork.Empresa.ObterMetaAccessToken(command.IdEmpresa);

                // 3. Dispara a criação para a API da Meta
                // Esse método retorna a string JSON de resposta contendo o ID gerado pela Meta
                var respostaMetaJson = await _metaService.CriarTemplateMetaAsync(command.NomeTemplate, command.Idioma ?? "pt_BR", command.Categoria, componentesMeta, wabaId, token);

                if (string.IsNullOrEmpty(respostaMetaJson))
                {
                    response.AddErro("A Meta aceitou o template, mas retornou uma resposta vazia.");
                    return response;
                }

                criadoNaMeta = true;

                // A Meta devolve o id numerico gerado pro template recem-criado -- precisa ser
                // guardado porque a edicao (POST /{template-id}) e a exclusao mais precisa exigem
                // esse id, nao o nome.
                var metaTemplateId = Newtonsoft.Json.Linq.JObject.Parse(respostaMetaJson)["id"]?.ToString();

                // 4. Cria a entidade de Domínio para salvar no banco local
                // Nota: Por padrão, todo template recém-criado entra em análise na Meta com o status "PENDING"
                var novoTemplate = new Entidades.Template
                {
                    Id = Guid.NewGuid(),
                    EmpresaId = command.IdEmpresa,
                    NomeTemplate = command.NomeTemplate,
                    NomeExibicao = string.IsNullOrWhiteSpace(command.NomeExibicao) ? null : command.NomeExibicao.Trim(),
                    Conteudo = command.Conteudo,
                    Categoria = command.Categoria,
                    Idioma = command.Idioma ?? "pt_BR",
                    Status = "PENDING",
                    MetaTemplateId = metaTemplateId,
                    GeraCobranca = command.GeraCobranca,
                    DataCriacao = DateTime.Now
                };

                var componentesLocais = TemplateComponentesBuilder.MontarComponentesLocais(command);

                if (componentesLocais.Any())
                {
                    novoTemplate.Componentes = componentesLocais;
                }

                // 5. Persiste as alterações via Unit of Work
                await _unitOfWork.Template.Incluir(novoTemplate);

                // 6. Retorna o resultado mapeado para o padrão do seu Use Case
                response.AddValue(new CriaTemplateResult(novoTemplate));
                _unitOfWork.Commit();
            }
            catch (Exception ex)
            {
                _unitOfWork.Rollback();

                // A Meta nao tem rollback: se o template ja foi criado la e so a gravacao local
                // falhou, ele fica orfao e toda nova tentativa com o mesmo nome e recusada pela
                // Meta como duplicado (caso real: BD/49 nao aplicada em prod). O "Atualizar da
                // Meta" importa o orfao, entao orienta o usuario a usar isso em vez de recriar.
                if (criadoNaMeta)
                {
                    _logger.LogError(ex, "Template {NomeTemplate} criado na Meta, mas nao foi salvo localmente para a empresa {IdEmpresa}.", command.NomeTemplate, command.IdEmpresa);
                    response.AddErro($"O modelo \"{command.NomeTemplate}\" foi criado na Meta, mas não foi salvo na plataforma. Não crie de novo: use \"Atualizar da Meta\" para trazê-lo para a lista.");
                    return response;
                }

                // Captura e formata erros de HttpClient da Meta ou falhas no banco local
                response.AddErroServico(ex, _logger, nameof(CriaTemplateHandler));
            }

            return response;
        }
    }
}


