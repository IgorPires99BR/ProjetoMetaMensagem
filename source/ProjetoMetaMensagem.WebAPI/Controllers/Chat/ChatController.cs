using Microsoft.AspNetCore.Mvc;
using ProjetoMetaMensagem.Dominio.Entidades;
using ProjetoMetaMensagem.Dominio.Interfaces;
using ProjetoMetaMensagem.Dominio.Interfaces.Servicos;

namespace ProjetoMetaMensagem.WebAPI.Controllers.Chat
{
    [ApiController]
    [Route("api/chat")]
    public class ChatController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMetaService _metaService;
        private readonly IGeminiService _geminiService;

        public ChatController(IUnitOfWork unitOfWork, IMetaService metaService, IGeminiService geminiService)
        {
            _unitOfWork = unitOfWork;
            _metaService = metaService;
            _geminiService = geminiService;
        }

        // GET /api/chat/conversas/{empresaId}
        // Lista conversas agrupadas por contato com ultima mensagem e nao-lidas
        [HttpGet("conversas/{empresaId}")]
        public async Task<IActionResult> ListarConversas(Guid empresaId)
        {
            try
            {
                var mensagens = await _unitOfWork.MensagemRecebida.ListarPorEmpresa(empresaId);
                var contatos = await _unitOfWork.Contato.Obter();
                var contatoMap = contatos.ToDictionary(c => c.Id);

                // Agrupa por contato
                var conversas = mensagens
                    .GroupBy(m => m.ContatoId)
                    .Select(g =>
                    {
                        var ultima = g.OrderByDescending(m => m.DataRecebimento).First();
                        var contato = g.Key.HasValue && contatoMap.ContainsKey(g.Key.Value)
                            ? contatoMap[g.Key.Value]
                            : null;

                        return new
                        {
                            ContatoId = g.Key,
                            Nome = contato?.Nome ?? ultima.TelefoneRemetente,
                            Telefone = ultima.TelefoneRemetente,
                            UltimaMensagem = ultima.Conteudo,
                            DataUltimaMensagem = ultima.DataRecebimento,
                            NaoLidas = g.Count(m => !m.Lida && m.Tipo == "recebida"),
                            Online = false
                        };
                    })
                    .OrderByDescending(c => c.DataUltimaMensagem)
                    .ToList();

                return Ok(new { value = conversas });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { erro = ex.Message });
            }
        }

        // GET /api/chat/mensagens/{empresaId}/{contatoId}
        // Historico de mensagens de um contato
        [HttpGet("mensagens/{empresaId}/{contatoId}")]
        public async Task<IActionResult> ListarMensagens(Guid empresaId, Guid contatoId)
        {
            try
            {
                var mensagens = await _unitOfWork.MensagemRecebida.ListarPorContato(empresaId, contatoId);

                var result = mensagens.Select(m => new
                {
                    m.Id,
                    m.Conteudo,
                    m.Tipo, // "recebida" ou "enviada"
                    m.DataRecebimento,
                    m.Lida
                });

                return Ok(new { value = result });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { erro = ex.Message });
            }
        }

        // POST /api/chat/enviar
        // Envia mensagem de texto para um contato via WhatsApp
        [HttpPost("enviar")]
        public async Task<IActionResult> EnviarMensagem([FromBody] EnviarMensagemRequest request)
        {
            try
            {
                if (string.IsNullOrEmpty(request.Telefone) || string.IsNullOrEmpty(request.Mensagem))
                    return BadRequest(new { erro = "Telefone e mensagem sao obrigatorios" });

                // Busca token e phoneNumberId da empresa
                var phoneNumberId = await _unitOfWork.Empresa.ObterPhoneNumberId(request.EmpresaId);
                var token = await _unitOfWork.Empresa.ObterMetaAccessToken(request.EmpresaId);

                // Envia via API da Meta
                var sucesso = await _metaService.EnviarTextoLivreAsync(request.Telefone, request.Mensagem);

                // Salva no historico
                var mensagem = new MensagemRecebida
                {
                    EmpresaId = request.EmpresaId,
                    ContatoId = request.ContatoId,
                    TelefoneRemetente = request.Telefone,
                    Conteudo = request.Mensagem,
                    Tipo = "enviada",
                    Lida = true
                };

                await _unitOfWork.MensagemRecebida.Incluir(mensagem);

                return Ok(new { value = new { sucesso, mensagemId = mensagem.Id } });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { erro = ex.Message });
            }
        }

        // POST /api/chat/sugerir/{empresaId}
        // Sugere uma resposta usando IA baseada na ultima mensagem do cliente
        [HttpPost("sugerir/{empresaId}")]
        public async Task<IActionResult> SugerirResposta(Guid empresaId, [FromBody] SugerirRequest request)
        {
            try
            {
                // Busca historico recente para contexto
                var contexto = "";
                if (request.ContatoId.HasValue)
                {
                    var historico = await _unitOfWork.MensagemRecebida.ListarPorContato(empresaId, request.ContatoId.Value);
                    var ultimas = historico
                        .OrderByDescending(m => m.DataRecebimento)
                        .Take(5)
                        .Select(m => $"{(m.Tipo == "recebida" ? "Cliente" : "Voce")}: {m.Conteudo}")
                        .Reverse();

                    contexto = string.Join("\n", ultimas);
                }

                var sugestao = await _geminiService.SugerirResposta(request.MensagemCliente, contexto);
                return Ok(new { value = sugestao });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { erro = ex.Message });
            }
        }

        // PUT /api/chat/marcar-lida/{mensagemId}
        [HttpPut("marcar-lida/{mensagemId}")]
        public async Task<IActionResult> MarcarComoLida(Guid mensagemId)
        {
            try
            {
                await _unitOfWork.MensagemRecebida.MarcarComoLida(mensagemId);
                return Ok(new { value = true });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { erro = ex.Message });
            }
        }
    }

    public class EnviarMensagemRequest
    {
        public Guid EmpresaId { get; set; }
        public Guid? ContatoId { get; set; }
        public string Telefone { get; set; }
        public string Mensagem { get; set; }
    }

    public class SugerirRequest
    {
        public Guid? ContatoId { get; set; }
        public string MensagemCliente { get; set; }
    }
}
