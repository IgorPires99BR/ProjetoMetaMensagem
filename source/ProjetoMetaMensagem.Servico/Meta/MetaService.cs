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

            // Ajustado para v19.0 como no seu Python
            _httpClient.BaseAddress = new Uri(_configuration.BaseUrl);
            _httpClient.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", _configuration.AccessToken);
        }

        #region CONFIGURACOES / INTEGRACOES MULTI-TENANT

        public async Task<string?> BuscarWabaIdDaMetaAsync(string accessToken)
        {
            try
            {
                // Endpoint relativo aproveitando a versão injetada na BaseUrl (Ex: https://graph.facebook.com/v19.0/)
                var endpoint = "me/whatsapp_business_accounts";

                // Sobrescreve localmente a autenticação apenas para usar o token temporário/estendido fornecido pela empresa externa
                var request = new HttpRequestMessage(HttpMethod.Get, endpoint);
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

                var response = await _httpClient.SendAsync(request);
                var responseContent = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    throw new Exception($"Erro na API da Meta ao tentar puxar WABA ID: {responseContent}");
                }

                var resultado = JsonConvert.DeserializeObject<BuscarWabaIDMetaResponse>(responseContent);

                // Captura e retorna o ID da primeira conta comercial vinculada àquele token
                return resultado?.Data?.FirstOrDefault()?.Id;
            }
            catch (Exception ex)
            {
                throw new Exception($"Falha ao consultar WabaId na API da Meta: {ex.Message}");
            }
        }

        #endregion

        #region NUMEROS
        public async Task<ObtemNumerosMetaResposta> ObterNumerosMetaAsync(string wabaId)
        {
            var endpoint = $"{wabaId}/phone_numbers";

            try
            {
                var response = await _httpClient.GetAsync(endpoint);
                var responseContent = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    throw new Exception($"Erro ao obter números da Meta: {responseContent}");
                }

                var metaResponse = JsonConvert.DeserializeObject<ObtemNumerosMetaResponse>(responseContent);

                // 2. Realiza o "De/Para" para a classe de Domínio
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

        public async Task<CriaNumeroMetaResposta> CriarNumeroMetaAsync(CriaNumeroMetaRequisicao requisicao, string wabaId)
        {

            var endpoint = $"{wabaId}/phone_numbers";

            CriaNumeroMetaRequest request = new CriaNumeroMetaRequest(requisicao);

            var json = JsonConvert.SerializeObject(request);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            try
            {
                var response = await _httpClient.PostAsync(endpoint, content);
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
        public async Task<EnviarMensagemTemplateResposta> EnviarTemplateAsync(EnviarMensagemTemplateRequisicao requisicao)
        {

            var requestMeta = new EnviarMensagemTemplateRequest
            {
                To = requisicao.Para,
                Template = new TemplateDataRequest
                {
                    Name = requisicao.Template.Nome,
                    Language = new LanguageDataRequest { Code = requisicao.Template.Idioma?.Codigo },
                    // Converte os componentes tipados do domínio para a lista genérica aceita pela infraestrutura
                    Components = requisicao.Template.Componentes?.Cast<object>().ToList()
                }
            };

            // 2. Serializa ignorando campos nulos (importante para não enviar 'sub_type' em textos simples)
            var settings = new JsonSerializerSettings
            {
                ContractResolver = new Newtonsoft.Json.Serialization.CamelCasePropertyNamesContractResolver(),
                NullValueHandling = NullValueHandling.Ignore
            };

            var json = JsonConvert.SerializeObject(requestMeta, settings);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            // 3. Dispara para o endpoint da Meta usando o PhoneNumberId da sua configuração
            var response = await _httpClient.PostAsync($"{_configuration.PhoneNumberId}/messages", content);
            var responseContent = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                // Em vez de estourar uma exception que quebra o lote todo, devolvemos o objeto encapsulando a falha
                return new EnviarMensagemTemplateResposta
                {
                    Sucesso = false,
                    Erro = $"Erro na API da Meta: {responseContent}"
                };
            }

            // Deserializa o JSON de sucesso usando a sua nova classe Response da Meta
            var metaResponse = JsonConvert.DeserializeObject<EnviarMensagemTemplateResponse>(responseContent);
            var wamid = metaResponse?.Messages?.FirstOrDefault()?.Id;

            return new EnviarMensagemTemplateResposta
            {
                Sucesso = true,
                WamidMeta = wamid
            };
        }

        public async Task<ObtemTemplatesMetaResposta> ObterTemplatesMetaAsync(string wabaId)
        {
            var endpoint = $"{wabaId}/message_templates";

            try
            {
                var response = await _httpClient.GetAsync(endpoint);
                var responseContent = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    throw new Exception($"Erro ao obter templates da Meta: {responseContent}");
                }

                // 1. Desserializa para o padrão bruto da Meta (Infraestrutura)
                var metaResponse = JsonConvert.DeserializeObject<ObtemTemplateMetaResponse>(responseContent);

                // 2. Realiza o "De/Para" mapeando e limpando os dados para o Domínio
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
        //Só é possivel enviar mensagem com texto Livre Quando há uma janela de conversa aberta
        public async Task<bool> EnviarTextoLivreAsync(string celular, string mensagem)
        {
            // 1. Monta o objeto seguindo a estrutura exata da Meta para texto livre
            var payload = new MetaTextMessageRequest
            {
                To = celular,
                Text = new TextContent
                {
                    PreviewUrl = true,
                    Body = mensagem
                }
            };

            // 2. Serializa (usando as configurações do Newtonsoft para respeitar os nomes da Meta)
            var json = JsonConvert.SerializeObject(payload);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            // 3. Envia para o endpoint de mensagens
            var response = await _httpClient.PostAsync($"{_configuration.PhoneNumberId}/messages", content);

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                // Log ou tratamento de erro
                throw new Exception($"Erro na API da Meta: {errorContent}");
            }

            return response.IsSuccessStatusCode;
        }

        public async Task<string> CriarTemplateMetaAsync(CreateTemplateRequisicao novoTemplate)
        {
            // IMPORTANTE: Use o WABA_ID aqui, não o PhoneNumberId
            var wabaId = _configuration.WabaID;

            var endpoint = $"https://graph.facebook.com/v20.0/{wabaId}/message_templates";

            // Serializa garantindo que campos nulos não sejam enviados (como 'format' vazio)
            var json = JsonConvert.SerializeObject(novoTemplate, new JsonSerializerSettings
            {
                NullValueHandling = NullValueHandling.Ignore,
                ContractResolver = new Newtonsoft.Json.Serialization.DefaultContractResolver
                {
                    NamingStrategy = new Newtonsoft.Json.Serialization.SnakeCaseNamingStrategy()
                }
            });

            var content = new StringContent(json, Encoding.UTF8, "application/json");

            // Adicione o Header de Autenticação se não estiver no HttpClient global
            // _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _configuration.AccessToken);

            var response = await _httpClient.PostAsync(endpoint, content);
            var responseContent = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                throw new Exception($"Erro ao criar template na Meta: {responseContent}");
            }

            return responseContent;
        }

        public async Task<Dictionary<string, EnviarMensagemTemplateResposta>> EnviarTemplatesEmLoteAsync(EnviarMensagemTemplateLoteRequisicao requisicaoLote)
        {
            var resultadoLote = new Dictionary<string, EnviarMensagemTemplateResposta>();

            // 1. Explode o lote utilizando a inteligência da sua classe de domínio
            var requisicoesIndividuais = requisicaoLote.GerarRequisicoesIndividuais();

            // 2. Processa os envios em paralelo aproveitando a concorrência do HttpClient
            var tarefas = requisicoesIndividuais.Select(async req =>
            {
                try
                {
                    // Mapeia o objeto do seu domínio diretamente para o DTO de Request do Serviço de Infraestrutura
                    var requestMeta = new EnviarMensagemTemplateRequisicao
                    {
                        Para = req.Para,
                        Template = new TemplateData
                        {
                            Nome = req.Template.Nome,
                            Idioma = new LanguageData { Codigo = req.Template.Idioma?.Codigo },
                            // Cast simples ou mapeamento dos componentes gerados pela classe de domínio
                            Componentes = req.Template.Componentes.Cast<ComponenteEnvio>().ToList()
                        }
                    };

                    // Reaproveita a lógica que dispara para a API da Meta e extrai o wamid
                    var respostaIndividual = await EnviarTemplateAsync(requestMeta);

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

            // 3. Alimenta o dicionário limpando chaves duplicadas
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
