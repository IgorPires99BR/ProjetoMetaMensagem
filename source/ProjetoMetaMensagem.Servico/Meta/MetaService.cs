using ProjetoMetaMensagem.Dominio.Interfaces.Servicos;
using Newtonsoft.Json;
using System.Net.Http.Headers;
using System.Text;
using ProjetoMetaMensagem.Dominio.Entidades.Servico.Meta;
using ProjetoMetaMensagem.Servico.Configuration;
using Microsoft.Extensions.Options;
using ProjetoMetaMensagem.Servico.Meta.Numeros.ObtemNumerosMeta;
using ProjetoMetaMensagem.Dominio.Entidades.Servico.Meta.Template;
using ProjetoMetaMensagem.Dominio.Entidades.Servico.Meta.Template.EnviarMensagemTemplate;
using ProjetoMetaMensagem.Servico.Meta.Numeros.CriaNumeroMeta;
using ProjetoMetaMensagem.Dominio.Entidades.Servico.Meta.Numeros.ObtemNumerosMeta;
using ProjetoMetaMensagem.Dominio.Entidades.Servico.Meta.Numeros.CriaNumeroMeta;
using ProjetoMetaMensagem.Servico.Meta.Templates.ObtemTemplateMeta;
using ProjetoMetaMensagem.Dominio.Entidades.Servico.Meta.Template.ObtemTemplateMeta;
using ProjetoMetaMensagem.Servico.Meta.WhatsappAccount.BuscarWabaIDMeta;
using ProjetoMetaMensagem.Dominio.Entidades.Servico.Meta.Template.EnviarMensagemTemplateLote;
using ProjetoMetaMensagem.Servico.Meta.Mensagens.EnviarMensagemTemplate;
using ProjetoMetaMensagem.Dominio.Common;

namespace ProjetoMetaMensagem.Servico.Meta
{
    public class MetaService : IMetaService
    {
        private readonly HttpClient _httpClient;
        private readonly ApiWhatsappConnectionConfiguration _configuration;

        public MetaService(HttpClient httpClient, IOptions<ApiWhatsappConnectionConfiguration> options)
        {
            _httpClient = httpClient;
            _configuration = options.Value;

            // Mantém a BaseUrl injetada (Ex: https://graph.facebook.com/v19.0/)
            _httpClient.BaseAddress = new Uri(_configuration.BaseUrl);

            // REMOVIDO: O cabeçalho Authorization global foi removido daqui para suportar multi-tenancy dinâmico.
        }

        #region CONFIGURACOES / INTEGRACOES MULTI-TENANT

        public async Task<string?> BuscarWabaIdDaMetaAsync(string accessToken)
        {
            try
            {
                // Endpoint que lista as contas comerciais do WhatsApp pertencentes ao Token fornecido
                var endpoint = "me/whatsapp_business_accounts";

                var request = new HttpRequestMessage(HttpMethod.Get, endpoint);
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

                var response = await _httpClient.SendAsync(request);
                var responseContent = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    throw new Exception($"Erro na API da Meta ao tentar puxar WABA ID: {responseContent}");
                }

                var resultado = JsonConvert.DeserializeObject<BuscarWabaIDMetaResponse>(responseContent);

                // Verifica se a estrutura retornada possui dados populados
                if (resultado?.Data == null || !resultado.Data.Any())
                {
                    // Retorna null explicitamente caso o token seja válido mas não possua WABA criado/vinculado
                    return null;
                }

                // Retorna o ID da primeira conta comercial encontrada no gerenciador
                return resultado.Data.First().Id;
            }
            catch (Exception ex)
            {
                throw new Exception($"Falha ao consultar WabaId na API da Meta: {ex.Message}");
            }
        }

        #endregion

        #region NUMEROS

        // Ajustado: Adicionado parâmetro accessToken e aplicando Authorization localmente na requisição
        public async Task<ObtemNumerosMetaResposta> ObterNumerosMetaAsync(string wabaId, string accessToken)
        {
            var endpoint = $"{wabaId}/phone_numbers";

            try
            {
                var request = new HttpRequestMessage(HttpMethod.Get, endpoint);
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

                var response = await _httpClient.SendAsync(request);
                var responseContent = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    throw new Exception($"Erro ao obter números da Meta: {responseContent}");
                }

                var metaResponse = JsonConvert.DeserializeObject<ObtemNumerosMetaResponse>(responseContent);

                var resultado = new ObtemNumerosMetaResposta
                {
                    Numeros = metaResponse.Data.Select(n => new NumeroMetaDto
                    {
                        Id = n.Id,
                        NumeroFormatado = n.DisplayPhoneNumber,
                        NomeVerificado = n.VerifiedName,
                        Status = n.Status,
                        Qualidade = n.QualityRating,
                        CodigoPais = n.CountryCode,
                        EhContaOficial = n.AccountMode == "LIVE"
                    }).ToList()
                };

                return resultado;
            }
            catch (Exception ex)
            {
                throw new Exception($"Falha ao consultar números na Meta: {ex.Message}");
            }
        }

        // Ajustado: Adicionado parâmetro accessToken e aplicando Authorization localmente na requisição
        public async Task<CriaNumeroMetaResposta> CriarNumeroMetaAsync(CriaNumeroMetaRequisicao requisicao, string wabaId, string accessToken)
        {
            var endpoint = $"{wabaId}/phone_numbers";

            CriaNumeroMetaRequest requestMeta = new CriaNumeroMetaRequest(requisicao);
            var json = JsonConvert.SerializeObject(requestMeta);

            try
            {
                var request = new HttpRequestMessage(HttpMethod.Post, endpoint);
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
                request.Content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await _httpClient.SendAsync(request);
                var responseContent = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    throw new Exception($"Erro ao criar/vincular número na Meta: {responseContent}");
                }

                var resultado = JsonConvert.DeserializeObject<CriaNumeroMetaResponse>(responseContent);

                CriaNumeroMetaResposta resposta = new CriaNumeroMetaResposta()
                {
                    Id = resultado.Id
                };

                return resposta;
            }
            catch (Exception ex)
            {
                throw new Exception($"Falha na comunicação de criação com a Meta: {ex.Message}");
            }
        }
        #endregion

        #region TEMPLATES

        public async Task<EnviarMensagemTemplateResposta> EnviarTemplateAsync(EnviarMensagemTemplateRequisicao requisicao, string phoneNumberId, string accessToken)
        {
            var requestMeta = new EnviarMensagemTemplateRequest
            {
                To = requisicao.Para,
                Template = new TemplateDataRequest
                {
                    Name = requisicao.Template.Nome,
                    Language = new LanguageDataRequest { Code = requisicao.Template.Idioma?.Codigo },
                    Components = requisicao.Template.Componentes?.Select(c => new ComponentDataRequest
                    {
                        Type = c.Tipo,
                        SubType = c.SubTipo,
                        Index = c.Indice,
                        Parameters = c.Parametros?.Select(p => new ParameterDataRequest
                        {
                            Type = p.Tipo,
                            Text = p.Texto
                        }).ToList()
                    }).ToList()
                }
            };

            var settings = new JsonSerializerSettings
            {
                NullValueHandling = NullValueHandling.Ignore
            };

            var json = JsonConvert.SerializeObject(requestMeta, settings);

            // ATENÇÃO: Como este método usa o "PhoneNumberId" do painel Master/Configuração padrão para envios, 
            // ele usa o Token Master vindo do appsettings. Ele usa HttpRequestMessage para garantir o isolamento do token.
            var request = new HttpRequestMessage(HttpMethod.Post, $"{phoneNumberId}/messages");
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
            request.Content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.SendAsync(request);
            var responseContent = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                return new EnviarMensagemTemplateResposta
                {
                    Sucesso = false,
                    Erro = $"Erro na API da Meta: {responseContent}"
                };
            }

            var metaResponse = JsonConvert.DeserializeObject<EnviarMensagemTemplateResponse>(responseContent);
            var wamid = metaResponse?.Messages?.FirstOrDefault()?.Id;

            return new EnviarMensagemTemplateResposta
            {
                Sucesso = true,
                WamidMeta = wamid
            };
        }

        // Ajustado: Adicionado parâmetro accessToken e aplicando Authorization localmente na requisição
        public async Task<ObtemTemplatesMetaResposta> ObterTemplatesMetaAsync(string wabaId, string accessToken)
        {
            var endpoint = $"{wabaId}/message_templates";

            try
            {
                var request = new HttpRequestMessage(HttpMethod.Get, endpoint);
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

                var response = await _httpClient.SendAsync(request);
                var responseContent = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    throw new Exception($"Erro ao obter templates da Meta: {responseContent}");
                }

                var metaResponse = JsonConvert.DeserializeObject<ObtemTemplateMetaResponse>(responseContent);

                var resultado = new ObtemTemplatesMetaResposta
                {
                    Templates = metaResponse.Data.Select(t => new TemplateMetaDto
                    {
                        Id = t.Id,
                        Nome = t.Name,
                        Status = t.Status,
                        Categoria = t.Category,
                        Idioma = t.Language,
                        ConteudoCorpo = t.Components?
                            .FirstOrDefault(c => c.Type.Equals("BODY", StringComparison.OrdinalIgnoreCase))?
                            .Text ?? string.Empty
                    }).ToList()
                };

                return resultado;
            }
            catch (Exception ex)
            {
                throw new Exception($"Falha ao consultar templates na Meta: {ex.Message}");
            }
        }
        #endregion

        #region MENSAGENS

        public async Task<bool> EnviarTextoLivreAsync(string celular, string mensagem)
        {
            var payload = new MetaTextMessageRequest
            {
                To = celular,
                Text = new TextContent
                {
                    PreviewUrl = true,
                    Body = mensagem
                }
            };

            var json = JsonConvert.SerializeObject(payload);

            var request = new HttpRequestMessage(HttpMethod.Post, $"{_configuration.PhoneNumberId}/messages");
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _configuration.AccessToken);
            request.Content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.SendAsync(request);

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                throw new Exception($"Erro na API da Meta: {errorContent}");
            }

            return response.IsSuccessStatusCode;
        }

        // Ajustado: Modificado para receber accessToken como parâmetro em vez de ler fixo do _configuration
        public async Task<string> CriarTemplateMetaAsync(CreateTemplateRequisicao novoTemplate, string wabaId, string accessToken)
        {
            var endpoint = $"https://graph.facebook.com/v20.0/{wabaId}/message_templates";

            var json = JsonConvert.SerializeObject(novoTemplate, new JsonSerializerSettings
            {
                NullValueHandling = NullValueHandling.Ignore,
                ContractResolver = new Newtonsoft.Json.Serialization.DefaultContractResolver
                {
                    NamingStrategy = new Newtonsoft.Json.Serialization.SnakeCaseNamingStrategy()
                }
            });

            var request = new HttpRequestMessage(HttpMethod.Post, endpoint);
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
            request.Content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.SendAsync(request);
            var responseContent = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                throw new Exception($"Erro ao criar template na Meta: {responseContent}");
            }

            return responseContent;
        }

        public async Task<Dictionary<string, EnviarMensagemTemplateResposta>> EnviarTemplatesEmLoteAsync(EnviarMensagemTemplateLoteRequisicao requisicaoLote, string phoneNumberId, string accessToken)
        {
            var resultadoLote = new Dictionary<string, EnviarMensagemTemplateResposta>();
            var requisicoesIndividuais = requisicaoLote.GerarRequisicoesIndividuais();

            var tarefas = requisicoesIndividuais.Select(async req =>
            {
                try
                {
                    var requestMeta = new EnviarMensagemTemplateRequisicao
                    {
                        Para = req.Para,
                        Template = new TemplateData
                        {
                            Nome = req.Template.Nome,
                            Idioma = new LanguageData { Codigo = req.Template.Idioma?.Codigo },
                            Componentes = req.Template.Componentes.Cast<ComponenteEnvio>().ToList()
                        }
                    };

                    var respostaIndividual = await EnviarTemplateAsync(requestMeta, phoneNumberId, accessToken);
                    return new { Telefone = req.Para, Resposta = respostaIndividual };
                }
                catch (Exception ex)
                {
                    return new
                    {
                        Telefone = req.Para,
                        Resposta = new EnviarMensagemTemplateResposta
                        {
                            Sucesso = false,
                            Erro = ex.Message
                        }
                    };
                }
            });

            var respostas = await Task.WhenAll(tarefas);

            foreach (var item in respostas)
            {
                if (!resultadoLote.ContainsKey(item.Telefone))
                {
                    resultadoLote.Add(item.Telefone, item.Resposta);
                }
            }

            return resultadoLote;
        }

        #endregion
    }
}