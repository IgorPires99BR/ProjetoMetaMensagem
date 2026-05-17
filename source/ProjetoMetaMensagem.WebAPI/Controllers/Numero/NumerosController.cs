using Microsoft.AspNetCore.Mvc;
using ProjetoMetaMensagem.Dominio.Interfaces.Mediator;
using ProjetoMetaMensagem.Dominio.UseCases.Numero.CriaNumero;
using ProjetoMetaMensagem.Dominio.UseCases.Numero.AtualizaNumeroMeta;
using ProjetoMetaMensagem.WebAPI.Common;
using System.Net;
using ProjetoMetaMensagem.Dominio.UseCases.Numero.ListarNumeros;

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
            var resultado = await _mediator.Send(command);
            return this.ValidateResponse((int)HttpStatusCode.Created, resultado);
        }

        [HttpGet("api/numero/ListarNumeros/{usuarioId}")]
        public async Task<IActionResult> ListarNumeros(Guid usuarioId)
        {
            var resultado = await _mediator.Send(new ListarNumerosCommand(usuarioId));
            return this.ValidateResponse((int)HttpStatusCode.OK, resultado);
        }

        [HttpGet("api/numero/AtualizarNumerosMeta/{usuarioId}")]
        public async Task<IActionResult> AtualizarNumerosMeta(Guid usuarioId)
        {
            var resultado = await _mediator.Send(new AtualizaNumeroMetaCommand(usuarioId));
            return this.ValidateResponse((int)HttpStatusCode.OK, resultado);
        }
    }
}
