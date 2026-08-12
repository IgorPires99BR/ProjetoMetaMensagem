using ProjetoMetaMensagem.Dominio.Help.Error;
using Microsoft.AspNetCore.Mvc;
using ProjetoMetaMensagem.Dominio.Interfaces.Mediator;
using ProjetoMetaMensagem.Dominio.UseCases.IA.SugerirTexto;

namespace ProjetoMetaMensagem.WebAPI.Controllers.IA
{
    // Endpoint generico do assistente de IA (Gemini) reutilizado por Chats, Templates,
    // Flows e Disparador -- cada tela manda sua propria instrucao/contexto, aqui so
    // delega pro mediator. Sem escopo de empresa: nao le nem grava nada por tenant, so
    // conversa com o Gemini.
    [ApiController]
    [Route("api/ia")]
    public class IAController : Controller
    {
        private readonly IMediator _mediator;
        private readonly ILogger<IAController> _logger;

        public IAController(IMediator mediator, ILogger<IAController> logger)
        {
            _mediator = mediator;
            _logger = logger;
        }

        // POST /api/ia/sugerir-texto
        [HttpPost("sugerir-texto")]
        public async Task<IActionResult> SugerirTexto([FromBody] SugerirTextoCommand command)
        {
            try
            {
                var resultado = await _mediator.Send(command);

                if (resultado == null || resultado.Erros.Count > 0)
                {
                    return BadRequest(new { erros = resultado?.Erros });
                }

                return Ok(new { value = resultado.Value });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { mensagem = TratamentoErro.Tratar(ex, _logger, "IAController.SugerirTexto"), tipo = "Servico" });
            }
        }
    }
}
