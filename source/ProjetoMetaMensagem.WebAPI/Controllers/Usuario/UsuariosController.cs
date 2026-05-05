using Microsoft.AspNetCore.Mvc;
using ProjetoMetaMensagem.Dominio.Interfaces.Mediator;
using ProjetoMetaMensagem.Dominio.UseCases.Usuario.AlteraUsuario;
using ProjetoMetaMensagem.Dominio.UseCases.Usuario.CriaUsuario;
using ProjetoMetaMensagem.Dominio.UseCases.Usuario.DeletaUsuario;
using ProjetoMetaMensagem.Dominio.UseCases.Usuario.ObtemUsuario;
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
        public async Task<IActionResult> Incluir([FromBody] CriaUsuarioCommand command)
        {
            var resultado = await _mediator.Send(command);
            return this.ValidateResponse(resultado != null ? (int)HttpStatusCode.Created : (int)HttpStatusCode.BadRequest, resultado);
        }

        [HttpDelete("api/usuario/excluir/{id}")]
        public async Task<IActionResult> Deletar(Guid id)
        {
            DeletaUsuarioCommand command = new DeletaUsuarioCommand(id);

            var resultado = await _mediator.Send(command);
            return this.ValidateResponse(resultado != null ? (int)HttpStatusCode.Created : (int)HttpStatusCode.BadRequest, resultado);
        }

        [HttpPut("api/usuario/alterar")]
        public async Task<IActionResult> Alterar([FromBody] AlteraUsuarioCommand command)
        {
            var resultado = await _mediator.Send(command);
            return this.ValidateResponse(resultado != null ? (int)HttpStatusCode.Created : (int)HttpStatusCode.BadRequest, resultado);
        }

        [HttpGet("api/usuario/obter-por-empresa/{idEmpresa}")]
        public async Task<IActionResult> ObterPorId(Guid idEmpresa)
        {
            var resultado = await _mediator.Send(new ObtemUsuarioCommand(idEmpresa));
            return this.ValidateResponse(resultado != null ? (int)HttpStatusCode.OK : (int)HttpStatusCode.NotFound, resultado);
        }
    }
}
