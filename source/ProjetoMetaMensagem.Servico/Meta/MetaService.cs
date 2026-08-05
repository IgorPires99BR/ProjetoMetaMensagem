using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using ProjetoMetaMensagem.Dominio.Common;
using ProjetoMetaMensagem.Dominio.Entidades;
using ProjetoMetaMensagem.Dominio.Entidades.Servico.Meta;
using ProjetoMetaMensagem.Dominio.Entidades.Servico.Meta.Numeros.AtivaCoexistencia;
using ProjetoMetaMensagem.Dominio.Entidades.Servico.Meta.Numeros.CriaNumeroMeta;
using ProjetoMetaMensagem.Dominio.Entidades.Servico.Meta.Numeros.EmbeddedSignup;
using ProjetoMetaMensagem.Dominio.Entidades.Servico.Meta.Numeros.ObtemNumerosMeta;
using ProjetoMetaMensagem.Dominio.Entidades.Servico.Meta.Template;
using ProjetoMetaMensagem.Dominio.Entidades.Servico.Meta.Template.EnviarMensagemTemplate;
using ProjetoMetaMensagem.Dominio.Entidades.Servico.Meta.Template.EnviarMensagemTemplateLote;
using ProjetoMetaMensagem.Dominio.Entidades.Servico.Meta.Template.ObtemTemplateMeta;
using ProjetoMetaMensagem.Dominio.Enums;
using ProjetoMetaMensagem.Dominio.Interfaces.Servicos;
using ProjetoMetaMensagem.Servico.Configuration;
using ProjetoMetaMensagem.Servico.Meta.Mensagens.EnviarMensagemTemplate;
using ProjetoMetaMensagem.Servico.Meta.Numeros.AtivaCoexistencia;
using ProjetoMetaMensagem.Servico.Meta.Numeros.CriaNumeroMeta;
using ProjetoMetaMensagem.Servico.Meta.Numeros.EmbeddedSignup;
using ProjetoMetaMensagem.Servico.Meta.Numeros.ObtemNumerosMeta;
using ProjetoMetaMensagem.Servico.Meta.Templates.ObtemTemplateMeta;
using ProjetoMetaMensagem.Servico.Meta.WhatsappAccount.BuscarWabaIDMeta;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

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
        }

        #region CONFIGURACOES / INTEGRACOES MULTI-TENANT

        public async Task<string?> BuscarWabaIdDaMetaAsync(string accessToken)
        {
            try
            {
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

                if (resultado?.Data == null || !resultado.Data.Any())
                {
                    return null;
                }

                return resultado.Data.First().Id;
            }
            catch (Exception ex)
            {
                throw new Exception($"Falha ao consultar WabaId na API da Meta: {ex.Message}");
            }
        }

        #endregion

        #region NUMEROS

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
        public async Task<TrocaCodeMetaResposta> TrocarCodeEmbeddedSignupAsync(string code)
        {
            var endpoint = $"oauth/access_token?client_id={_configuration.AppId}&client_secret={_configuration.AppSecret}&code={code}";

            try
            {
                var request = new HttpRequestMessage(HttpMethod.Get, endpoint);

                var response = await _httpClient.SendAsync(request);
                var responseContent = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    throw new Exception($"Erro ao trocar code do Embedded Signup: {responseContent}");
                }

                var metaResponse = JsonConvert.DeserializeObject<TrocaCodeMetaResponse>(responseContent);

                return new TrocaCodeMetaResposta
                {
                    SystemUserToken = metaResponse.AccessToken
                };
            }
            catch (Exception ex)
            {
                throw new Exception($"Falha ao trocar code do Embedded Signup na Meta: {ex.Message}");
            }
        }

        public async Task<AtivaCoexistenciaMetaResposta> AtivarCoexistenciaAsync(string phoneNumberId, string accessToken, string pin)
        {
            var endpoint = $"{phoneNumberId}/register";

            var requestMeta = new AtivaCoexistenciaRequest { Pin = pin };
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
                    return new AtivaCoexistenciaMetaResposta
                    {
                        Sucesso = false,
                        Erro = responseContent
                    };
                }

                var metaResponse = JsonConvert.DeserializeObject<AtivaCoexistenciaResponse>(responseContent);

                return new AtivaCoexistenciaMetaResposta
                {
                    Sucesso = metaResponse.Success,
                    StatusConexao = metaResponse.Success ? "Conectado" : "Erro"
                };
            }
            catch (Exception ex)
            {
                return new AtivaCoexistenciaMetaResposta
                {
                    Sucesso = false,
                    Erro = $"Falha na comunicação de ativação de coexistência com a Meta: {ex.Message}"
                };
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
                            Text = p.Tipo == "text" ? p.Texto : null,
                            Image = p.Tipo == "image" ? new MediaLinkDataRequest { Link = p.Texto } : null,
                            Video = p.Tipo == "video" ? new MediaLinkDataRequest { Link = p.Texto } : null,
                            Document = p.Tipo == "document" ? new MediaLinkDataRequest { Link = p.Texto } : null
                        }).ToList()
                    }).ToList()
                }
            };

            var settings = new JsonSerializerSettings
            {
                NullValueHandling = NullValueHandling.Ignore
            };

            var json = JsonConvert.SerializeObject(requestMeta, settings);

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
                    Erro = responseContent
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
                            .Text ?? string.Empty,

                        // CHADA CORRETA: Executa a varredura e transformação estruturada
                        Componentes = MapearComponentesMeta(t.Components)
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

        public async Task<bool> EnviarTextoLivreAsync(string celular, string mensagem, string accessToken, string phoneNumberId)
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

            var idNumeroEnvio = !string.IsNullOrEmpty(phoneNumberId) ? phoneNumberId : _configuration.PhoneNumberId;

            var request = new HttpRequestMessage(HttpMethod.Post, $"{idNumeroEnvio}/messages");
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
            request.Content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.SendAsync(request);

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                throw new Exception($"Erro na API da Meta: {errorContent}");
            }

            return response.IsSuccessStatusCode;
        }

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

            var jsonSettings = new JsonSerializerSettings
            {
                NullValueHandling = NullValueHandling.Ignore
            };

            string jsonPayload = JsonConvert.SerializeObject(requisicoesIndividuais.FirstOrDefault(), jsonSettings);

            var tarefas = requisicoesIndividuais.Select(async req =>
            {
                try
                {
                    var respostaIndividual = await EnviarTemplateAsync(req, phoneNumberId, accessToken);
                    respostaIndividual.ContatoId = req.ContatoId;
                    respostaIndividual.JsonEnviado = jsonPayload;

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

        #region MAPEAMENTO INTERNO

        /// <summary>
        /// Traduz a árvore complexa da API da Meta para a estrutura simplificada do nosso Domínio
        /// </summary>
        private List<TemplateComponenteDto> MapearComponentesMeta(List<TemplateComponentResponse> componentesMeta)
        {
            var componentesDominio = new List<TemplateComponenteDto>();
            if (componentesMeta == null) return componentesDominio;

            foreach (var comp in componentesMeta)
            {
                var dto = new TemplateComponenteDto();
                string typeUpper = comp.Type?.ToUpper() ?? string.Empty;

                if (typeUpper == "HEADER")
                {
                    dto.Tipo = TipoComponenteTemplate.Header;
                    dto.Texto = comp.Text;
                    dto.FormatMidia = comp.Format?.ToUpper() switch
                    {
                        "TEXT" => TipoMidiaTemplate.Text,
                        "IMAGE" => TipoMidiaTemplate.Image,
                        "VIDEO" => TipoMidiaTemplate.Video,
                        "DOCUMENT" => TipoMidiaTemplate.Document,
                        _ => TipoMidiaTemplate.None
                    };
                }
                else if (typeUpper == "BODY")
                {
                    dto.Tipo = TipoComponenteTemplate.Body;
                    dto.Texto = comp.Text;
                }
                else if (typeUpper == "FOOTER")
                {
                    dto.Tipo = TipoComponenteTemplate.Footer;
                    dto.Texto = comp.Text;
                }
                else if (typeUpper == "BUTTONS" && comp.Buttons != null)
                {
                    dto.Tipo = TipoComponenteTemplate.Buttons;
                    dto.Botoes = comp.Buttons.Select(b => new TemplateBotaoDto
                    {
                        Texto = b.Text,
                        Url = b.Url,
                        Tipo = b.Type?.ToUpper() switch
                        {
                            "QUICK_REPLY" => TipoBotaoTemplate.QuickReply,
                            "URL" => TipoBotaoTemplate.Url,
                            "PHONE_NUMBER" => TipoBotaoTemplate.PhoneNumber,
                            _ => TipoBotaoTemplate.QuickReply
                        }
                    }).ToList();
                }
                else
                {
                    continue; // Ignora tipos de blocos desconhecidos pela nossa regra de negócio
                }

                componentesDominio.Add(dto);
            }

            return componentesDominio;
        }
        #endregion
    }
}