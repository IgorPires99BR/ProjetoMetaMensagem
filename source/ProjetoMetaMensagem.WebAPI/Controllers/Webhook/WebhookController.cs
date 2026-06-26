using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using ProjetoMetaMensagem.Dominio.Entidades;
using ProjetoMetaMensagem.Dominio.Interfaces;
using ProjetoMetaMensagem.Dominio.Interfaces.Servicos;

namespace ProjetoMetaMensagem.WebAPI.Controllers.Webhook
{
    [ApiController]
    [Route("api/webhook")]
    public class WebhookController : Controller
    {
        private readonly IFlowOrchestratorService _flowOrchestrator;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMetaService _metaService;

        public WebhookController(IFlowOrchestratorService flowOrchestrator, IUnitOfWork unitOfWork, IMetaService metaService)
        {
            _flowOrchestrator = flowOrchestrator;
            _unitOfWork = unitOfWork;
            _metaService = metaService;
        }

        // GET api/webhook?hub.mode=subscribe&hub.challenge=123&hub.verify_token=TOKEN
        // Usado pela Meta para verificar o webhook
        [HttpGet]
        public IActionResult VerificarWebhook(
            [FromQuery(Name = "hub.mode")] string mode,
            [FromQuery(Name = "hub.challenge")] string challenge,
            [FromQuery(Name = "hub.verify_token")] string verifyToken)
        {
            try
            {
                if (string.IsNullOrEmpty(mode) || string.IsNullOrEmpty(challenge) || string.IsNullOrEmpty(verifyToken))
                    return BadRequest("Parametros invalidos.");

                if (mode == "subscribe")
                {
                    return Content(challenge, "text/plain");
                }

                return BadRequest("Modo invalido.");
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { erro = ex.Message, detalhe = ex.InnerException?.Message });
            }
        }

        // POST api/webhook
        // Recebe notificacoes da Meta (mensagens recebidas, status de entrega, etc.)
        [HttpPost]
        public async Task<IActionResult> ReceberNotificacao([FromBody] object payload)
        {
            try
            {
                var json = JsonConvert.SerializeObject(payload);
                var root = JObject.Parse(json);

                // Navega na estrutura: entry[].changes[].value.messages[]
                var entries = root["entry"] as JArray;
                if (entries == null) return Ok();

                foreach (var entry in entries)
                {
                    var changes = entry["changes"] as JArray;
                    if (changes == null) continue;

                    foreach (var change in changes)
                    {
                        var value = change["value"];
                        if (value == null) continue;

                        var metadata = value["metadata"];
                        var phoneNumberId = metadata?["phone_number_id"]?.ToString();

                        var messages = value["messages"] as JArray;
                        if (messages == null) continue;

                        foreach (var msg in messages)
                        {
                            var from = msg["from"]?.ToString(); // telefone do remetente
                            var msgType = msg["type"]?.ToString();
                            var text = msg["text"]?["body"]?.ToString();
                            var msgId = msg["id"]?.ToString();

                            if (string.IsNullOrEmpty(from) || string.IsNullOrEmpty(msgType))
                                continue;

                            // 1. Tenta encontrar o contato pelo telefone
                            var contatos = await _unitOfWork.Contato.Obter();
                            var contato = contatos.FirstOrDefault(c =>
                                c.Telefone != null && c.Telefone.Replace(" ", "").Replace("-", "").Contains(from.Replace("55", "")));

                            // 2. Salva a mensagem recebida
                            var mensagem = new MensagemRecebida
                            {
                                EmpresaId = Guid.Empty, // Será resolvido pelo phoneNumberId posteriormente
                                ContatoId = contato?.Id,
                                TelefoneRemetente = from,
                                Conteudo = text ?? $"[mensagem do tipo {msgType}]",
                                Tipo = "recebida",
                                Lida = false
                            };

                            await _unitOfWork.MensagemRecebida.Incluir(mensagem);

                            // 3. Se encontrou contato e empresa, tenta processar flow
                            if (contato != null && !string.IsNullOrEmpty(text))
                            {
                                // Busca a empresa do contato (multi-tenant)
                                // TODO: associar empresaId ao telefone de destino
                                // Por hora, processa sem empresa definida
                                try
                                {
                                    var resultado = await _flowOrchestrator.ProcessarMensagem(
                                        Guid.Empty, contato.Id, text);

                                    if (resultado.Sucesso && !string.IsNullOrEmpty(resultado.Mensagem))
                                    {
                                        await _metaService.EnviarTextoLivreAsync(from, resultado.Mensagem);
                                    }
                                }
                                catch
                                {
                                    // Flow pode nao estar configurado - ignora silenciosamente
                                }
                            }
                        }
                    }
                }

                return Ok();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erro no webhook: {ex.Message}");
                return Ok(); // Sempre retorna 200 para a Meta nao reenviar
            }
        }
    }
}
