using Microsoft.AspNetCore.Mvc;
using ProjetoMetaMensagem.Dominio.Interfaces.Mediator;
using ProjetoMetaMensagem.Dominio.UseCases.Numero.CriaNumero;
using ProjetoMetaMensagem.Dominio.UseCases.Numero.AtualizaNumeroMeta;
using ProjetoMetaMensagem.WebAPI.Common;
using System.Net;
using ProjetoMetaMensagem.Dominio.UseCases.Numero.ListarNumeros;
using ProjetoMetaMensagem.Dominio.UseCases.Numero.IniciaEmbeddedSignup;
using ProjetoMetaMensagem.Dominio.UseCases.Numero.AtivaCoexistencia;
using ProjetoMetaMensagem.Dominio.UseCases.Numero.DeletaNumero;

namespace ProjetoMetaMensagem.WebAPI.Controllers.Numero
{
    [ApiController]
    public class NumerosController : Controller
    {
        private readonly IMediator _mediator;
        public NumerosController(IMediator mediator) => _mediator = mediator;

        [HttpPost("api/numero/incluir")]
        public async Task<IActionResult> Incluir([FromBody] CriaNumeroCommand command)
        {
            try
            {
                var resultado = await _mediator.Send(command);
                return this.ValidateResponse((int)HttpStatusCode.Created, resultado);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { erro = ex.Message, detalhe = ex.InnerException?.Message });
            }
        }

        [HttpGet("api/numero/ListarNumeros/{usuarioId}")]
        public async Task<IActionResult> ListarNumeros(Guid usuarioId)
        {
            try
            {
                var resultado = await _mediator.Send(new ListarNumerosCommand(usuarioId));
                return this.ValidateResponse((int)HttpStatusCode.OK, resultado);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { erro = ex.Message, detalhe = ex.InnerException?.Message });
            }
        }

        [HttpPost("api/numero/AtualizarNumerosMeta/{usuarioId}")]
        public async Task<IActionResult> AtualizarNumerosMeta(Guid usuarioId, Guid idEmpresa)
        {
            try
            {
                var resultado = await _mediator.Send(new AtualizaNumeroMetaCommand(usuarioId, idEmpresa));
                return this.ValidateResponse((int)HttpStatusCode.OK, resultado);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { erro = ex.Message, detalhe = ex.InnerException?.Message });
            }
        }

        [HttpPost("api/numero/embedded-signup")]
        public async Task<IActionResult> EmbeddedSignup([FromBody] IniciaEmbeddedSignupCommand command)
        {
            try
            {
                var resultado = await _mediator.Send(command);
                return this.ValidateResponse((int)HttpStatusCode.Created, resultado);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { erro = ex.Message, detalhe = ex.InnerException?.Message });
            }
        }

        [HttpPost("api/numero/ativa-coexistencia")]
        public async Task<IActionResult> AtivaCoexistencia(Guid numeroId, Guid idEmpresa, [FromBody] AtivaCoexistenciaRequestBody body)
        {
            try
            {
                var resultado = await _mediator.Send(new AtivaCoexistenciaCommand(numeroId, idEmpresa, body?.Pin));
                return this.ValidateResponse((int)HttpStatusCode.OK, resultado);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { erro = ex.Message, detalhe = ex.InnerException?.Message });
            }
        }

        [HttpDelete("api/numero/excluir/{id}")]
        public async Task<IActionResult> Excluir(Guid id)
        {
            try
            {
                var resultado = await _mediator.Send(new DeletaNumeroCommand { Id = id });
                return this.ValidateResponse((int)HttpStatusCode.OK, resultado);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { erro = ex.Message, detalhe = ex.InnerException?.Message });
            }
        }
    }

    public class AtivaCoexistenciaRequestBody
    {
        public string? Pin { get; set; }
    }
}
