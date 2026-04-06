using Microsoft.AspNetCore.Mvc;
using ProjetoMetaMensagem.Dominio.Interfaces.Mediator;
using ProjetoMetaMensagem.WebAPI.Common;
using System.Net;

namespace ProjetoMetaMensagem.WebAPI.Controllers.Usuario
{
    [ApiController]
    public class UsuariosController : Controller
    {
        private readonly IMediator _mediator;
        public UsuariosController(IMediator mediator) => _mediator = mediator;

        [HttpPost("api/usuario/incluir")]
        public async Task<IActionResult> Incluir([FromBody] IncluirUsuarioCommand command)
        {
            var resultado = await _mediator.Send(command);
            return this.ValidateResponse(resultado != null ? (int)HttpStatusCode.Created : (int)HttpStatusCode.BadRequest, resultado);
        }

        [HttpGet("api/usuario/obter-por-id/{id}")]
        public async Task<IActionResult> ObterPorId(string id)
        {
            var resultado = await _mediator.Send(new ObterUsuarioPorIdQuery { Id = id });
            return this.ValidateResponse(resultado != null ? (int)HttpStatusCode.OK : (int)HttpStatusCode.NotFound, resultado);
        }
    }
}
