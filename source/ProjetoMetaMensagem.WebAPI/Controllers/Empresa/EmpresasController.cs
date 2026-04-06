using Microsoft.AspNetCore.Mvc;
using ProjetoMetaMensagem.Dominio.Interfaces.Mediator;
using ProjetoMetaMensagem.Dominio.UseCases.Admin.Companies.AlteraEmpresa;
using ProjetoMetaMensagem.WebAPI.Common;
using System.Net;

namespace ProjetoMetaMensagem.WebAPI.Controllers.Empresa
{
    [ApiController]
    public class EmpresasController : Controller
    {
        private readonly IMediator _mediator;

        public EmpresasController(IMediator mediator) => _mediator = mediator;

        [HttpPost("api/empresa/incluir")]
        public async Task<IActionResult> Incluir([FromBody] IncluirEmpresaCommand command)
        {
            var resultado = await _mediator.Send(command);
            return this.ValidateResponse(resultado != null ? (int)HttpStatusCode.Created : (int)HttpStatusCode.BadRequest, resultado);
        }

        [HttpPut("api/empresa/alterar")]
        public async Task<IActionResult> Alterar([FromBody] AlterarEmpresaCommand command)
        {
            var resultado = await _mediator.Send(command);
            return this.ValidateResponse((int)HttpStatusCode.OK, resultado);
        }

        [HttpDelete("api/empresa/excluir/{id}")]
        public async Task<IActionResult> Excluir(string id)
        {
            var resultado = await _mediator.Send(new ExcluirEmpresaCommand { Id = id });
            return this.ValidateResponse((int)HttpStatusCode.OK, resultado);
        }

        [HttpGet("api/empresa/obter-por-id/{id}")]
        public async Task<IActionResult> ObterPorId(string id)
        {
            var resultado = await _mediator.Send(new ObterEmpresaPorIdQuery { Id = id });
            return this.ValidateResponse(resultado != null ? (int)HttpStatusCode.OK : (int)HttpStatusCode.NotFound, resultado);
        }

        [HttpGet("api/empresa/obter")]
        public async Task<IActionResult> Obter()
        {
            var resultado = await _mediator.Send(new ObterEmpresasQuery());
            return this.ValidateResponse((int)HttpStatusCode.OK, resultado);
        }
    }
}
