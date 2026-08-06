using Microsoft.AspNetCore.Mvc;
using ProjetoMetaMensagem.Dominio.Entidades;
using ProjetoMetaMensagem.Dominio.Interfaces;
using ProjetoMetaMensagem.Dominio.Interfaces.Mediator;
using ProjetoMetaMensagem.Dominio.Interfaces.Servicos;
using ProjetoMetaMensagem.Dominio.UseCases.MensagemRecebida.ListaChatsAtivos;
using ProjetoMetaMensagem.Dominio.UseCases.MensagemRecebida.ListaMensagemRecebida;
using ProjetoMetaMensagem.Dominio.UseCases.MensagemRecebida.MarcarComoLida;

namespace ProjetoMetaMensagem.WebAPI.Controllers.Chat
{
    [ApiController]
    [Route("api/chat")]
    public class ChatController : Controller
    {
        private readonly IMediator _mediator;
        
        public ChatController(IMediator mediator)
        {
            _mediator = mediator;
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
                return StatusCode(500, new { erro = ex.Message, detalhe = ex.InnerException?.Message });
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
                return StatusCode(500, new { erro = ex.Message, detalhe = ex.InnerException?.Message });
            }
        }

        [HttpPost("marcar-como-lida")]
        public async Task<IActionResult> MarcarComoLida([FromBody] MarcarComoLidaRequest request)
        {
            if (request == null || request.EmpresaId == Guid.Empty || request.ContatoId == Guid.Empty)
            {
                return BadRequest(new { erro = "EmpresaId e ContatoId inválidos." });
            }

            try
            {
                var command = new MarcarComoLidaCommand { EmpresaId = request.EmpresaId, ContatoId = request.ContatoId };
                var resultado = await _mediator.Send(command);

                if (resultado == null || resultado.Erros.Count > 0)
                {
                    return BadRequest(new { erro = "Não foi possível marcar as mensagens como lidas." });
                }

                return Ok(new { sucesso = true });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { erro = ex.Message });
            }
        }
    }

    public class MarcarComoLidaRequest
    {
        public Guid EmpresaId { get; set; }
        public Guid ContatoId { get; set; }
    }
}
