using ProjetoMetaMensagem.Dominio.Help.Error;
using Microsoft.AspNetCore.Mvc;
using ProjetoMetaMensagem.Dominio.Entidades;
using ProjetoMetaMensagem.Dominio.Interfaces;
using ProjetoMetaMensagem.Dominio.Interfaces.Mediator;
using ProjetoMetaMensagem.Dominio.Interfaces.Servicos;
using ProjetoMetaMensagem.Dominio.UseCases.MensagemRecebida.DevolverAoBot;
using ProjetoMetaMensagem.Dominio.UseCases.MensagemRecebida.ListaChatsAtivos;
using ProjetoMetaMensagem.Dominio.UseCases.MensagemRecebida.ListaMensagemRecebida;
using ProjetoMetaMensagem.Dominio.UseCases.MensagemRecebida.MarcarComoLida;
using ProjetoMetaMensagem.Dominio.UseCases.MensagemRecebida.ObtemMidia;

namespace ProjetoMetaMensagem.WebAPI.Controllers.Chat
{
    [ApiController]
    [Route("api/chat")]
    public class ChatController : Controller
    {
        private readonly IMediator _mediator;
        
        private readonly ILogger<ChatController> _logger;

        public ChatController(IMediator mediator, ILogger<ChatController> logger)
        {
            _mediator = mediator;
            _logger = logger;
        }

        // GET /api/chat/conversas/{empresaId}
        // Lista conversas agrupadas por contato com ultima mensagem e nao-lidas
        [HttpGet("conversas/{idEmpresa}")]
        public async Task<IActionResult> ListarConversas(Guid idEmpresa, Guid contatoId)
        {
            try
            {
                // Criando o Command/Query para buscar as conversas delegando a regra de negócio e agrupamento para o Handler
                var command = new ListaChatsAtivosCommand(idEmpresa,contatoId);
                var resultado = await _mediator.Send(command);

                if (resultado == null)
                {
                    return BadRequest(new { erros = resultado.Erros });
                }

                return Ok(new { value = resultado.Value });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { mensagem = TratamentoErro.Tratar(ex, _logger, "ChatController.ListarConversas"), tipo = "Servico" });
            }
        }

        // GET /api/chat/mensagens/{empresaId}/{contatoId}
        // Histórico de mensagens de um contato via Mediator
        [HttpGet("mensagens/{idEmpresa}/{contatoId}")]
        public async Task<IActionResult> ListarMensagens(Guid idEmpresa, Guid contatoId, int pagina = 0, int tamanho = 30)
        {
            try
            {
                // Disparando o Command que criamos e mapeamos anteriormente
                var command = new ListaMensagemRecebidaCommand
                {
                    EmpresaId = idEmpresa,
                    ContatoId = contatoId,
                    Pagina = pagina,
                    TamanhoPagina = tamanho
                };

                var resultado = await _mediator.Send(command);

                if (resultado == null)
                {
                    return BadRequest(new { erros = resultado.Erros });
                }

                // O resultado mapeado (com as propriedades From, Text, Time) vai direto para o Angular
                return Ok(new { value = resultado.Value.Mensagens });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { mensagem = TratamentoErro.Tratar(ex, _logger, "ChatController.ListarMensagens"), tipo = "Servico" });
            }
        }

        [HttpPost("marcar-como-lida")]
        public async Task<IActionResult> MarcarComoLida([FromBody] MarcarComoLidaRequest request)
        {
            if (request == null || request.EmpresaId == Guid.Empty || request.ContatoId == Guid.Empty)
            {
                return BadRequest(new { mensagem = "EmpresaId e ContatoId inválidos.", tipo = "Negocio" });
            }

            try
            {
                var command = new MarcarComoLidaCommand { EmpresaId = request.EmpresaId, ContatoId = request.ContatoId };
                var resultado = await _mediator.Send(command);

                if (resultado == null || resultado.Erros.Count > 0)
                {
                    return BadRequest(new { mensagem = "Não foi possível marcar as mensagens como lidas.", tipo = "Negocio" });
                }

                return Ok(new { sucesso = true });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { mensagem = TratamentoErro.Tratar(ex, _logger, "ChatController.MarcarComoLida"), tipo = "Servico" });
            }
        }
        // POST /api/chat/devolver-ao-bot
        // Reativa o flow automatico numa conversa que um vendedor tinha assumido manualmente
        [HttpPost("devolver-ao-bot")]
        public async Task<IActionResult> DevolverAoBot([FromBody] DevolverAoBotRequest request)
        {
            if (request == null || request.EmpresaId == Guid.Empty || request.ContatoId == Guid.Empty)
            {
                return BadRequest(new { mensagem = "EmpresaId e ContatoId inválidos.", tipo = "Negocio" });
            }

            try
            {
                var command = new DevolverAoBotCommand { EmpresaId = request.EmpresaId, ContatoId = request.ContatoId };
                var resultado = await _mediator.Send(command);

                if (resultado == null || resultado.Erros.Count > 0)
                {
                    return BadRequest(new { mensagem = "Não foi possível devolver a conversa ao flow.", tipo = "Negocio" });
                }

                return Ok(new { sucesso = true });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { mensagem = TratamentoErro.Tratar(ex, _logger, "ChatController.DevolverAoBot"), tipo = "Servico" });
            }
        }

        // GET /api/chat/midia/{midiaId}?empresaId=...
        // Proxy pra baixar a mídia da Meta sem expor o access token pro front
        [HttpGet("midia/{midiaId}")]
        public async Task<IActionResult> ObtemMidia(string midiaId, Guid empresaId)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(midiaId) || empresaId == Guid.Empty)
                {
                    return BadRequest(new { mensagem = "midiaId e empresaId são obrigatórios.", tipo = "Negocio" });
                }

                var command = new ObtemMidiaCommand { MidiaId = midiaId, EmpresaId = empresaId };
                var resultado = await _mediator.Send(command);

                if (resultado == null || resultado.Erros.Count > 0 || resultado.Value == null)
                {
                    return NotFound(new { mensagem = "Mídia não encontrada ou indisponível.", tipo = "Negocio" });
                }

                return File(resultado.Value.Bytes, resultado.Value.MimeType);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { mensagem = TratamentoErro.Tratar(ex, _logger, "ChatController.ObtemMidia"), tipo = "Servico" });
            }
        }
    }

    public class MarcarComoLidaRequest
    {
        public Guid EmpresaId { get; set; }
        public Guid ContatoId { get; set; }
    }

    public class DevolverAoBotRequest
    {
        public Guid EmpresaId { get; set; }
        public Guid ContatoId { get; set; }
    }
}
