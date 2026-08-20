using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using ProjetoMetaMensagem.Dominio.Enums;
using ProjetoMetaMensagem.Dominio.Help.Error;
using ProjetoMetaMensagem.Dominio.Interfaces.Servicos;
using ProjetoMetaMensagem.Dominio.Interfaces.Servicos.Meta;
using ProjetoMetaMensagem.Dominio.UseCases.Messages.EnviarMensagemTemplateMeta;
using ProjetoMetaMensagem.Dominio.UseCases.Messages.EnviarMensagemTemplateMetaLote;
using ProjetoMetaMensagem.Servico.Configuration;
using ProjetoMetaMensagem.Servico.MetaService.Wire;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace ProjetoMetaMensagem.Servico.MetaService
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

        public async Task<List<NumeroMetaDto>> ObterNumerosMetaAsync(string wabaId, string accessToken)
        {
            // O `fields` e obrigatorio: sem ele a Graph devolve so um subconjunto (id, telefone,
            // nome verificado, quality_rating) e `status` volta nulo -- era o que fazia o painel
            // mostrar "0 numeros ativos" com o numero conectado logo abaixo na propria lista.
            var endpoint = $"{wabaId}/phone_numbers?fields=id,display_phone_number,verified_name,status,quality_rating,code_verification_status,account_mode";

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

                // Sem esta guarda, um corpo inesperado da Meta virava "Object reference not set"
                // no log -- erro que nao diz nada sobre a causa real.
                if (metaResponse?.Data == null)
                {
                    throw new Exception($"Resposta inesperada da Meta ao obter números: {responseContent}");
                }

                return metaResponse.Data.Select(n => new NumeroMetaDto
                {
                    Id = n.Id,
                    NumeroFormatado = n.DisplayPhoneNumber,
                    NomeVerificado = n.VerifiedName,
                    Status = n.Status,
                    Qualidade = n.QualityRating,
                    CodigoPais = n.CountryCode,
                    EhContaOficial = n.AccountMode == "LIVE"
                }).ToList();
            }
            catch (Exception ex)
            {
                throw new Exception($"Falha ao consultar números na Meta: {ex.Message}");
            }
        }

        public async Task<string> CriarNumeroMetaAsync(string telefone, string nomeVerificado, string codigoPais, string wabaId, string accessToken)
        {
            var endpoint = $"{wabaId}/phone_numbers";

            var requestMeta = new CriaNumeroMetaRequest(telefone, nomeVerificado, codigoPais);
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
                    throw MetaApiException.DoCorpo(responseContent, "o cadastro do número");
                }

                var resultado = JsonConvert.DeserializeObject<CriaNumeroMetaResponse>(responseContent);

                if (resultado == null)
                {
                    throw new Exception($"Resposta inesperada da Meta ao criar/vincular número: {responseContent}");
                }

                return resultado.Id;
            }
            catch (MetaApiException)
            {
                // Recusa da propria Meta ja vem com a mensagem pro usuario: reembrulhar aqui
                // apagaria justamente o motivo que ele precisa ler.
                throw;
            }
            catch (Exception ex)
            {
                throw new Exception($"Falha na comunicação de criação com a Meta: {ex.Message}");
            }
        }

        public async Task<string> TrocarCodeEmbeddedSignupAsync(string code)
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

                if (metaResponse == null)
                {
                    throw new Exception($"Resposta inesperada da Meta ao trocar code do Embedded Signup: {responseContent}");
                }

                return metaResponse.AccessToken;
            }
            catch (Exception ex)
            {
                throw new Exception($"Falha ao trocar code do Embedded Signup na Meta: {ex.Message}");
            }
        }

        public async Task<(string Token, DateTime? ExpiraEm)> TrocarTokenLongLivedAsync(string shortLivedToken)
        {
            var endpoint = $"oauth/access_token?grant_type=fb_exchange_token&client_id={_configuration.AppId}&client_secret={_configuration.AppSecret}&fb_exchange_token={shortLivedToken}";

            try
            {
                var request = new HttpRequestMessage(HttpMethod.Get, endpoint);

                var response = await _httpClient.SendAsync(request);
                var responseContent = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    throw new Exception($"Erro ao trocar token por long-lived: {responseContent}");
                }

                var metaResponse = JsonConvert.DeserializeObject<TrocaTokenLongLivedResponse>(responseContent);

                if (metaResponse == null || string.IsNullOrEmpty(metaResponse.AccessToken))
                {
                    throw new Exception($"Resposta inesperada da Meta ao trocar token por long-lived: {responseContent}");
                }

                var expiraEm = metaResponse.ExpiresIn.HasValue
                    ? DateTime.Now.AddSeconds(metaResponse.ExpiresIn.Value)
                    : (DateTime?)null;

                return (metaResponse.AccessToken, expiraEm);
            }
            catch (Exception ex)
            {
                throw new Exception($"Falha ao trocar token por long-lived na Meta: {ex.Message}");
            }
        }

        // Sem body relevante: a Meta aceita POST vazio nesse endpoint (o WABA a assinar vem na
        // propria URL). Segue o mesmo estilo de AtivarCoexistenciaAsync -- nao lanca excecao em
        // erro HTTP, devolve false e deixa o caller decidir (logar/retry), pra uma falha aqui
        // nao derrubar o cadastro do numero que ja foi persistido.
        public async Task<bool> AssinarAppNoWabaAsync(string wabaId, string accessToken)
        {
            var endpoint = $"{wabaId}/subscribed_apps";

            try
            {
                var request = new HttpRequestMessage(HttpMethod.Post, endpoint);
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

                var response = await _httpClient.SendAsync(request);
                return response.IsSuccessStatusCode;
            }
            catch
            {
                return false;
            }
        }

        public async Task<ResultadoCoexistencia> AtivarCoexistenciaAsync(string phoneNumberId, string accessToken, string pin)
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
                    return new ResultadoCoexistencia
                    {
                        Sucesso = false,
                        Erro = responseContent
                    };
                }

                var metaResponse = JsonConvert.DeserializeObject<AtivaCoexistenciaResponse>(responseContent);

                if (metaResponse == null)
                {
                    return new ResultadoCoexistencia
                    {
                        Sucesso = false,
                        Erro = $"Resposta inesperada da Meta: {responseContent}"
                    };
                }

                return new ResultadoCoexistencia
                {
                    Sucesso = metaResponse.Success,
                    StatusConexao = metaResponse.Success ? "Conectado" : "Erro"
                };
            }
            catch (Exception ex)
            {
                return new ResultadoCoexistencia
                {
                    Sucesso = false,
                    Erro = $"Falha na comunicação de ativação de coexistência com a Meta: {ex.Message}"
                };
            }
        }
        #endregion

        #region TEMPLATES

        public async Task<ResultadoEnvioTemplate> EnviarTemplateAsync(EnviarMensagemTemplateMetaCommand command, string phoneNumberId, string accessToken)
        {
            var requestMeta = new EnviarMensagemTemplateRequest
            {
                To = TelefoneHelper.FormatarParaMeta(command.Telefone),
                Template = MontarTemplateData(command.NomeTemplate, command.Idioma, command.ParametroHeaderMediaUrl, command.ParametrosBody, command.ParametrosButton)
            };

            return await EnviarTemplateWireAsync(requestMeta, phoneNumberId, accessToken);
        }

        public async Task<string> UploadMidiaExemploAsync(string appId, string accessToken, byte[] arquivo, string mimeType)
        {
            try
            {
                // 1. Inicia a sessão de upload (endpoint normal, versionado -- usa a mesma BaseUrl
                // configurada dos demais métodos, evitando ficar preso numa versão hardcoded)
                var initUrl = $"{_configuration.BaseUrl}{appId}/uploads" +
                    $"?file_length={arquivo.Length}&file_type={Uri.EscapeDataString(mimeType)}&access_token={Uri.EscapeDataString(accessToken)}";

                var initRequest = new HttpRequestMessage(HttpMethod.Post, initUrl);
                var initResponse = await _httpClient.SendAsync(initRequest);
                var initContent = await initResponse.Content.ReadAsStringAsync();

                if (!initResponse.IsSuccessStatusCode)
                {
                    throw new Exception($"Erro ao iniciar upload de mídia na Meta: {initContent}");
                }

                var sessionId = JObject.Parse(initContent)["id"]?.ToString();
                if (string.IsNullOrEmpty(sessionId))
                {
                    throw new Exception("A Meta não retornou o ID da sessão de upload.");
                }

                // 2. Envia os bytes do arquivo pra sessão aberta -- o session-id já vem totalmente
                // qualificado pela Meta, essa chamada específica NÃO leva prefixo de versão.
                var uploadUrl = $"https://graph.facebook.com/{sessionId}";
                var uploadRequest = new HttpRequestMessage(HttpMethod.Post, uploadUrl);
                uploadRequest.Headers.TryAddWithoutValidation("Authorization", $"OAuth {accessToken}");
                uploadRequest.Headers.TryAddWithoutValidation("file_offset", "0");
                uploadRequest.Content = new ByteArrayContent(arquivo);
                uploadRequest.Content.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");

                var uploadResponse = await _httpClient.SendAsync(uploadRequest);
                var uploadContent = await uploadResponse.Content.ReadAsStringAsync();

                if (!uploadResponse.IsSuccessStatusCode)
                {
                    throw new Exception($"Erro ao enviar bytes do arquivo pra Meta: {uploadContent}");
                }

                var handle = JObject.Parse(uploadContent)["h"]?.ToString();
                if (string.IsNullOrEmpty(handle))
                {
                    throw new Exception("A Meta não retornou o handle do arquivo enviado.");
                }

                return handle;
            }
            catch (Exception ex)
            {
                throw new Exception($"Falha ao subir mídia de exemplo pra Meta: {ex.Message}");
            }
        }

        public async Task<List<TemplateMetaDto>> ObterTemplatesMetaAsync(string wabaId, string accessToken)
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

                if (metaResponse?.Data == null)
                {
                    throw new Exception($"Resposta inesperada da Meta ao obter templates: {responseContent}");
                }

                return metaResponse.Data.Select(t => new TemplateMetaDto
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
                }).ToList();
            }
            catch (Exception ex)
            {
                throw new Exception($"Falha ao consultar templates na Meta: {ex.Message}");
            }
        }
        #endregion

        #region MENSAGENS

        public async Task<string> EnviarTextoLivreAsync(string celular, string mensagem, string accessToken, string phoneNumberId)
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
            var responseContent = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                throw new Exception($"Erro na API da Meta: {responseContent}");
            }

            // Extrai o wamid retornado pela Meta (messages[0].id), usado depois pra
            // vincular os eventos de status de entrega (webhook) a esta mensagem.
            var responseObj = JObject.Parse(responseContent);
            return responseObj["messages"]?.FirstOrDefault()?["id"]?.ToString() ?? string.Empty;
        }

        public async Task<string> EnviarBotoesAsync(string celular, string corpo, List<string> botoes, string accessToken, string phoneNumberId)
        {
            // Limite da Meta: no maximo 3 botoes, titulo de ate 20 caracteres cada.
            var payload = new MetaInteractiveButtonsRequest
            {
                To = celular,
                Interactive = new InteractiveButtonsContent
                {
                    Body = new InteractiveBody { Text = corpo },
                    Action = new InteractiveButtonsAction
                    {
                        Buttons = botoes
                            .Where(b => !string.IsNullOrWhiteSpace(b))
                            .Take(3)
                            .Select((b, i) => new InteractiveButtonWrapper
                            {
                                Reply = new InteractiveButtonReply { Id = $"btn_{i}", Title = b.Length > 20 ? b.Substring(0, 20) : b }
                            })
                            .ToList()
                    }
                }
            };

            var json = JsonConvert.SerializeObject(payload);

            var idNumeroEnvio = !string.IsNullOrEmpty(phoneNumberId) ? phoneNumberId : _configuration.PhoneNumberId;

            var request = new HttpRequestMessage(HttpMethod.Post, $"{idNumeroEnvio}/messages");
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
            request.Content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.SendAsync(request);
            var responseContent = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                throw MetaApiException.DoCorpo(responseContent, "enviar a mensagem de escolha de plano");
            }

            var responseObj = JObject.Parse(responseContent);
            return responseObj["messages"]?.FirstOrDefault()?["id"]?.ToString() ?? string.Empty;
        }

        public async Task<string> CriarTemplateMetaAsync(string nome, string idioma, string categoria, List<ComponenteTemplateEnvio> componentes, string wabaId, string accessToken)
        {
            // Endpoint relativo a _httpClient.BaseAddress (config ApiWhatsappConnectionConfiguration.BaseUrl)
            // -- antes tinha a versao da Graph API hardcoded aqui, dessincronizada da BaseUrl configurada.
            var endpoint = $"{wabaId}/message_templates";

            var requestMeta = new CreateTemplateRequest(nome, idioma, categoria, componentes);

            var json = JsonConvert.SerializeObject(requestMeta, new JsonSerializerSettings
            {
                NullValueHandling = NullValueHandling.Ignore
            });

            var request = new HttpRequestMessage(HttpMethod.Post, endpoint);
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
            request.Content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.SendAsync(request);
            var responseContent = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                throw MetaApiException.DoCorpo(responseContent, "a criação do template");
            }

            return responseContent;
        }

        public async Task<string> AtualizarTemplateMetaAsync(string metaTemplateId, string categoria, List<ComponenteTemplateEnvio> componentes, string accessToken)
        {
            // A Meta edita um template existente via POST sobre o proprio id do template (nao sobre
            // o wabaId), e so aceita category + components no corpo -- name/language sao imutaveis
            // aqui (mudar idioma exige criar um template novo).
            var endpoint = $"{metaTemplateId}";

            var requestMeta = new EditTemplateRequest(categoria, componentes);

            var json = JsonConvert.SerializeObject(requestMeta, new JsonSerializerSettings
            {
                NullValueHandling = NullValueHandling.Ignore
            });

            var request = new HttpRequestMessage(HttpMethod.Post, endpoint);
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
            request.Content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.SendAsync(request);
            var responseContent = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                throw MetaApiException.DoCorpo(responseContent, "a alteração do template");
            }

            return responseContent;
        }

        public async Task<bool> ExcluirTemplateMetaAsync(string nomeTemplate, string? metaTemplateId, string wabaId, string accessToken)
        {
            var endpoint = $"{wabaId}/message_templates?name={Uri.EscapeDataString(nomeTemplate)}";
            if (!string.IsNullOrEmpty(metaTemplateId))
            {
                endpoint += $"&hsm_id={Uri.EscapeDataString(metaTemplateId)}";
            }

            var request = new HttpRequestMessage(HttpMethod.Delete, endpoint);
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

            var response = await _httpClient.SendAsync(request);
            var responseContent = await response.Content.ReadAsStringAsync();

            // 404/"template nao encontrado" e tratado como sucesso idempotente -- o template pode
            // ja ter sido apagado direto no Business Manager, e isso nao deve travar a exclusao local.
            if (!response.IsSuccessStatusCode)
            {
                if (response.StatusCode == System.Net.HttpStatusCode.NotFound ||
                    responseContent.Contains("does not exist", StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }

                throw MetaApiException.DoCorpo(responseContent, "a exclusão do template");
            }

            return true;
        }

        public async Task<Dictionary<string, ResultadoEnvioTemplate>> EnviarTemplatesEmLoteAsync(EnviarMensagemTemplateMetaLoteCommand comandoLote, string phoneNumberId, string accessToken)
        {
            var resultadoLote = new Dictionary<string, ResultadoEnvioTemplate>();

            var jsonSettings = new JsonSerializerSettings
            {
                NullValueHandling = NullValueHandling.Ignore
            };

            var telefones = comandoLote.Telefones ?? new List<string>();
            var contatosIds = comandoLote.ContatosIds ?? new List<string>();

            var tarefas = telefones.Select(async (telefone, index) =>
            {
                string contatoId = contatosIds.Count > index
                    ? contatosIds[index]
                    : "00000000-0000-0000-0000-000000000000";

                // O template e montado por destinatario: quando o disparo personaliza as
                // variaveis (nome de cada contato, por exemplo), cada um recebe os valores
                // dele; sem personalizacao, ParametrosBodyDe devolve os valores globais e o
                // payload sai identico ao de antes.
                var templateData = MontarTemplateData(
                    comandoLote.NomeTemplate,
                    comandoLote.Idioma,
                    comandoLote.ParametroHeaderMediaUrl,
                    comandoLote.ParametrosBodyDe(telefone),
                    comandoLote.ParametrosButton);

                var requestMeta = new EnviarMensagemTemplateRequest
                {
                    To = TelefoneHelper.FormatarParaMeta(telefone),
                    Template = templateData
                };

                try
                {
                    var resultadoIndividual = await EnviarTemplateWireAsync(requestMeta, phoneNumberId, accessToken);
                    resultadoIndividual.ContatoId = contatoId;
                    resultadoIndividual.JsonEnviado = JsonConvert.SerializeObject(templateData, jsonSettings);

                    return new { Telefone = telefone, Resultado = resultadoIndividual };
                }
                catch (Exception ex)
                {
                    return new
                    {
                        Telefone = telefone,
                        Resultado = new ResultadoEnvioTemplate
                        {
                            Sucesso = false,
                            Erro = ex.Message,
                            ContatoId = contatoId
                        }
                    };
                }
            });

            var respostas = await Task.WhenAll(tarefas);

            foreach (var item in respostas)
            {
                if (!resultadoLote.ContainsKey(item.Telefone))
                {
                    resultadoLote.Add(item.Telefone, item.Resultado);
                }
            }

            return resultadoLote;
        }

        public async Task<(byte[] Bytes, string MimeType)> BaixarMidiaAsync(string mediaId, string accessToken)
        {
            try
            {
                // 1. Consulta o metadado da mídia pra obter a URL temporária e o mime_type
                var metaRequest = new HttpRequestMessage(HttpMethod.Get, $"{_configuration.BaseUrl}{mediaId}");
                metaRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

                var metaResponse = await _httpClient.SendAsync(metaRequest);
                var metaContent = await metaResponse.Content.ReadAsStringAsync();

                if (!metaResponse.IsSuccessStatusCode)
                {
                    throw new Exception($"Erro ao obter metadados da mídia na Meta: {metaContent}");
                }

                var metaJson = JObject.Parse(metaContent);
                var url = metaJson["url"]?.ToString();
                var mimeType = metaJson["mime_type"]?.ToString() ?? "application/octet-stream";

                if (string.IsNullOrEmpty(url))
                {
                    throw new Exception("A Meta não retornou a URL da mídia.");
                }

                // 2. Baixa os bytes da mídia (a Meta exige o Bearer token também nessa chamada)
                var downloadRequest = new HttpRequestMessage(HttpMethod.Get, url);
                downloadRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

                var downloadResponse = await _httpClient.SendAsync(downloadRequest);

                if (!downloadResponse.IsSuccessStatusCode)
                {
                    var erroConteudo = await downloadResponse.Content.ReadAsStringAsync();
                    throw new Exception($"Erro ao baixar bytes da mídia na Meta: {erroConteudo}");
                }

                var bytes = await downloadResponse.Content.ReadAsByteArrayAsync();

                return (bytes, mimeType);
            }
            catch (Exception ex)
            {
                throw new Exception($"Falha ao baixar mídia da Meta: {ex.Message}");
            }
        }

        public async Task<(string WamidMeta, string MediaId)> EnviarMidiaAsync(string celular, byte[] arquivo, string mimeType, string tipoMidia, string accessToken, string phoneNumberId)
        {
            var idNumeroEnvio = !string.IsNullOrEmpty(phoneNumberId) ? phoneNumberId : _configuration.PhoneNumberId;

            try
            {
                // 1. Sobe o arquivo pro endpoint de mídia da Meta (multipart/form-data)
                using var form = new MultipartFormDataContent();
                form.Add(new StringContent("whatsapp"), "messaging_product");

                // A Meta só aceita o mime type "puro" (ex: audio/ogg) -- parâmetros extras
                // (ex: ";codecs=opus", vindos do MediaRecorder do navegador) fazem o upload
                // ser aceito e processado silenciosamente como falha, sem erro nenhum, e a
                // mensagem nunca sai do status "pendente" no destinatário.
                var mimeTypePuro = mimeType?.Split(';')[0].Trim() ?? mimeType;

                var fileContent = new ByteArrayContent(arquivo);
                fileContent.Headers.ContentType = new MediaTypeHeaderValue(mimeTypePuro);
                form.Add(fileContent, "file", "arquivo");

                var uploadRequest = new HttpRequestMessage(HttpMethod.Post, $"{idNumeroEnvio}/media");
                uploadRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
                uploadRequest.Content = form;

                var uploadResponse = await _httpClient.SendAsync(uploadRequest);
                var uploadContent = await uploadResponse.Content.ReadAsStringAsync();

                if (!uploadResponse.IsSuccessStatusCode)
                {
                    throw new Exception($"Erro ao subir mídia pra Meta: {uploadContent}");
                }

                var mediaId = JObject.Parse(uploadContent)["id"]?.ToString();
                if (string.IsNullOrEmpty(mediaId))
                {
                    throw new Exception("A Meta não retornou o ID da mídia enviada.");
                }

                // 2. Envia a mensagem referenciando a mídia recém-subida
                var payload = new JObject
                {
                    ["messaging_product"] = "whatsapp",
                    ["to"] = celular,
                    ["type"] = tipoMidia,
                    [tipoMidia] = new JObject { ["id"] = mediaId }
                };

                var json = payload.ToString();

                var request = new HttpRequestMessage(HttpMethod.Post, $"{idNumeroEnvio}/messages");
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
                request.Content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await _httpClient.SendAsync(request);
                var responseContent = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    throw new Exception($"Erro na API da Meta ao enviar mídia: {responseContent}");
                }

                var responseObj = JObject.Parse(responseContent);
                var wamid = responseObj["messages"]?.FirstOrDefault()?["id"]?.ToString() ?? string.Empty;

                return (wamid, mediaId);
            }
            catch (Exception ex)
            {
                throw new Exception($"Falha ao enviar mídia pra Meta: {ex.Message}");
            }
        }

        #endregion

        #region MAPEAMENTO INTERNO

        private async Task<ResultadoEnvioTemplate> EnviarTemplateWireAsync(EnviarMensagemTemplateRequest requestMeta, string phoneNumberId, string accessToken)
        {
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
                return new ResultadoEnvioTemplate
                {
                    Sucesso = false,
                    Erro = responseContent
                };
            }

            var metaResponse = JsonConvert.DeserializeObject<EnviarMensagemTemplateResponse>(responseContent);
            var wamid = metaResponse?.Messages?.FirstOrDefault()?.Id;

            return new ResultadoEnvioTemplate
            {
                Sucesso = true,
                WamidMeta = wamid
            };
        }

        /// <summary>
        /// Traduz os parâmetros (header de mídia, corpo e botão) do nosso Domínio para os
        /// componentes na estrutura exigida pela API de envio de template da Meta.
        /// </summary>
        private TemplateDataRequest MontarTemplateData(string nomeTemplate, string idioma, string? parametroHeaderMediaUrl, List<string>? parametrosBody, List<string>? parametrosButton)
        {
            return new TemplateDataRequest
            {
                Name = nomeTemplate,
                Language = new LanguageDataRequest { Code = idioma },
                Components = MontarComponentesEnvio(parametroHeaderMediaUrl, parametrosBody, parametrosButton)
            };
        }

        private List<ComponentDataRequest> MontarComponentesEnvio(string? parametroHeaderMediaUrl, List<string>? parametrosBody, List<string>? parametrosButton)
        {
            var componentes = new List<ComponentDataRequest>();

            // 1. Mapeia parâmetros do HEADER de mídia se existirem (Dinâmico por extensão de arquivo)
            if (!string.IsNullOrEmpty(parametroHeaderMediaUrl))
            {
                string tipoMidia = "image";
                string urlLower = parametroHeaderMediaUrl.ToLower();

                if (urlLower.EndsWith(".pdf") || urlLower.EndsWith(".doc") || urlLower.EndsWith(".docx"))
                    tipoMidia = "document";
                else if (urlLower.EndsWith(".mp4") || urlLower.EndsWith(".avi") || urlLower.EndsWith(".mov"))
                    tipoMidia = "video";

                var parametroHeader = new ParameterDataRequest
                {
                    Type = tipoMidia
                };

                // Atribui ao nó correto esperado pela API da Meta
                if (tipoMidia == "image")
                    parametroHeader.Image = new MediaLinkDataRequest { Link = parametroHeaderMediaUrl };
                else if (tipoMidia == "document")
                    parametroHeader.Document = new MediaLinkDataRequest { Link = parametroHeaderMediaUrl };
                else if (tipoMidia == "video")
                    parametroHeader.Video = new MediaLinkDataRequest { Link = parametroHeaderMediaUrl };

                componentes.Add(new ComponentDataRequest
                {
                    Type = "header",
                    Parameters = new List<ParameterDataRequest> { parametroHeader }
                });
            }

            // 2. Mapeia parâmetros do BODY se existirem
            if (parametrosBody != null && parametrosBody.Any())
            {
                componentes.Add(new ComponentDataRequest
                {
                    Type = "body",
                    Parameters = parametrosBody.Select(p => new ParameterDataRequest
                    {
                        Type = "text",
                        Text = p
                    }).ToList()
                });
            }

            // 3. Mapeia parâmetros de BUTTON se existirem
            if (parametrosButton != null && parametrosButton.Any(p => !string.IsNullOrWhiteSpace(p)))
            {
                componentes.Add(new ComponentDataRequest
                {
                    Type = "button",
                    SubType = "url",
                    Index = "0",
                    Parameters = parametrosButton
                        .Where(p => !string.IsNullOrWhiteSpace(p))
                        .Select(p => new ParameterDataRequest
                        {
                            Type = "text",
                            Text = p
                        }).ToList()
                });
            }

            return componentes;
        }

        /// <summary>
        /// Traduz a árvore complexa da API da Meta para a estrutura simplificada do nosso Domínio
        /// </summary>
        private List<ComponenteTemplateMetaDto> MapearComponentesMeta(List<TemplateComponentResponse> componentesMeta)
        {
            var componentesDominio = new List<ComponenteTemplateMetaDto>();
            if (componentesMeta == null) return componentesDominio;

            foreach (var comp in componentesMeta)
            {
                var dto = new ComponenteTemplateMetaDto();
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
                    dto.Botoes = comp.Buttons.Select(b => new BotaoTemplateMetaDto
                    {
                        Texto = b.Text,
                        Url = b.Url,
                        NumeroTelefone = b.PhoneNumber,
                        CodigoExemplo = b.Example,
                        Tipo = b.Type?.ToUpper() switch
                        {
                            "QUICK_REPLY" => TipoBotaoTemplate.QuickReply,
                            "URL" => TipoBotaoTemplate.Url,
                            "PHONE_NUMBER" => TipoBotaoTemplate.PhoneNumber,
                            "COPY_CODE" => TipoBotaoTemplate.CopyCode,
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
