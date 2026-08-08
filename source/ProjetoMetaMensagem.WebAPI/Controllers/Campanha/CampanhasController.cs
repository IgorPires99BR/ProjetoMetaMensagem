using ProjetoMetaMensagem.Dominio.Help.Error;
using Microsoft.AspNetCore.Mvc;
using ProjetoMetaMensagem.Dominio.Interfaces.Mediator;
using ProjetoMetaMensagem.Dominio.UseCases.Campanha.CancelaCampanha;
using ProjetoMetaMensagem.Dominio.UseCases.Campanha.CriaCampanha;
using ProjetoMetaMensagem.Dominio.UseCases.Campanha.ListaCampanha;
using ProjetoMetaMensagem.WebAPI.Common;
using System.Net;

namespace ProjetoMetaMensagem.WebAPI.Controllers.Campanha
{
    [ApiController]
    public class CampanhasController : Controller
    {
        private readonly IMediator _mediator;

        private readonly ILogger<CampanhasController> _logger;

        public CampanhasController(IMediator mediator, ILogger<CampanhasController> logger)
        {
            _mediator = mediator;
            _logger = logger;
        }

        [HttpPost("api/campanha/incluir")]
        public async Task<IActionResult> Incluir([FromBody] CriaCampanhaCommand command)
        {
            try
            {
                var resultado = await _mediator.Send(command);
                return this.ValidateResponse(resultado != null ? (int)HttpStatusCode.Created : (int)HttpStatusCode.BadRequest, resultado);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { erro = TratamentoErro.Tratar(ex, _logger, "CampanhasController.Incluir") });
            }
        }

        [HttpGet("api/campanha/listar/{empresaId}")]
        public async Task<IActionResult> Listar([FromRoute] Guid empresaId)
        {
            try
            {
                var resultado = await _mediator.Send(new ListaCampanhaCommand { EmpresaId = empresaId });
                return this.ValidateResponse((int)HttpStatusCode.OK, resultado);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { erro = TratamentoErro.Tratar(ex, _logger, "CampanhasController.Listar") });
            }
        }

        [HttpPut("api/campanha/cancelar/{id}")]
        public async Task<IActionResult> Cancelar([FromRoute] Guid id)
        {
            try
            {
                // Escopo vem do token, nunca da rota/corpo: senao o proprio atacante o escolheria.
                var resultado = await _mediator.Send(new CancelaCampanhaCommand
                {
                    Id = id,
                    EmpresaIdSolicitante = this.EmpresaDoEscopo()
                });
                return this.ValidateResponse((int)HttpStatusCode.OK, resultado);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { erro = TratamentoErro.Tratar(ex, _logger, "CampanhasController.Cancelar") });
            }
        }
    }
}
