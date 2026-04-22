using Microsoft.AspNetCore.Mvc;
using ProjetoMetaMensagem.Dominio.Interfaces.Mediator;
using ProjetoMetaMensagem.Dominio.UseCases.Contato.ObtemContato;
using ProjetoMetaMensagem.Dominio.UseCases.Numero.CriaNumero;
using ProjetoMetaMensagem.WebAPI.Common;
using System.Net;

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

        [HttpGet("api/numero/obter-por-usuario/{usuarioId}")]
        public async Task<IActionResult> ObterPorUsuario(string usuarioId)
        {
            var resultado = await _mediator.Send(new ObtemContatoCommand());
            return this.ValidateResponse((int)HttpStatusCode.OK, resultado);
        }
    }
}
