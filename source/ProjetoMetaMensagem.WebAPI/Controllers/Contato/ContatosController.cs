using Microsoft.AspNetCore.Mvc;
using ProjetoMetaMensagem.Dominio.Interfaces.Mediator;
using ProjetoMetaMensagem.Dominio.UseCases.Contato.CriaContato;
using ProjetoMetaMensagem.Dominio.UseCases.Contato.DeletaContato;
using ProjetoMetaMensagem.Dominio.UseCases.Contato.ObtemContato;
using ProjetoMetaMensagem.WebAPI.Common;
using System.Net;

namespace ProjetoMetaMensagem.WebAPI.Controllers.Contato
{
    [ApiController]
    public class ContatosController : Controller
    {
        private readonly IMediator _mediator;
        public ContatosController(IMediator mediator) => _mediator = mediator;

        [HttpPost("api/contato/incluir")]
        public async Task<IActionResult> Incluir([FromBody] CriaContatoCommand command)
        {
            var resultado = await _mediator.Send(command);
            return this.ValidateResponse(resultado != null ? (int)HttpStatusCode.Created : (int)HttpStatusCode.BadRequest, resultado);
        }

        [HttpGet("api/contato/obter-por-usuario/{usuarioId}")]
        public async Task<IActionResult> ObterPorUsuario(Guid usuarioId)
        {
            var resultado = await _mediator.Send(new ObtemContatoCommand(usuarioId));
            return this.ValidateResponse((int)HttpStatusCode.OK, resultado);
        }

        [HttpDelete("api/contato/excluir/{id}")]
        public async Task<IActionResult> Excluir(string id)
        {
            var resultado = await _mediator.Send(new DeletaContatoCommand { Id = id });
            return this.ValidateResponse((int)HttpStatusCode.OK, resultado);
        }
    }
}
