using Microsoft.AspNetCore.Mvc;
using ProjetoMetaMensagem.Dominio.Interfaces.Mediator;
using ProjetoMetaMensagem.WebAPI.Common;
using System.Net;

namespace ProjetoMetaMensagem.WebAPI.Controllers.Template
{
    [ApiController]
    public class TemplatesController : Controller
    {
        private readonly IMediator _mediator;
        public TemplatesController(IMediator mediator) => _mediator = mediator;

        [HttpPost("api/template/incluir")]
        public async Task<IActionResult> Incluir([FromBody] IncluirTemplateCommand command)
        {
            var resultado = await _mediator.Send(command);
            return this.ValidateResponse((int)HttpStatusCode.Created, resultado);
        }

        [HttpGet("api/template/obter-por-empresa/{empresaId}")]
        public async Task<IActionResult> ObterPorEmpresa(string empresaId)
        {
            var resultado = await _mediator.Send(new ObterTemplatesPorEmpresaQuery { EmpresaId = empresaId });
            return this.ValidateResponse((int)HttpStatusCode.OK, resultado);
        }

        [HttpPut("api/template/alterar")]
        public async Task<IActionResult> Alterar([FromBody] AlterarTemplateCommand command)
        {
            var resultado = await _mediator.Send(command);
            return this.ValidateResponse((int)HttpStatusCode.OK, resultado);
        }
    }
}
