using Microsoft.AspNetCore.Mvc;
using ProjetoMetaMensagem.Dominio.Interfaces.Mediator;
using ProjetoMetaMensagem.Dominio.UseCases.Template.AtualizaTemplateMeta;
using ProjetoMetaMensagem.Dominio.UseCases.Template.CriaTemplate;
using ProjetoMetaMensagem.Dominio.UseCases.Template.ListaTemplate;
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
        public async Task<IActionResult> Incluir([FromBody] CriaTemplateCommand command)
        {
            var resultado = await _mediator.Send(command);
            return this.ValidateResponse((int)HttpStatusCode.Created, resultado);
        }

        [HttpGet("api/template/Listar/{empresaId}")]
        public async Task<IActionResult> Listar(Guid empresaId)
        {
            var resultado = await _mediator.Send(new ListaTemplateCommand(empresaId));
            return this.ValidateResponse((int)HttpStatusCode.OK, resultado);
        }

        [HttpPut("api/template/AtualizaTemplateMeta/{empresaId}")]
        public async Task<IActionResult> Alterar(Guid empresaId)
        {
            var resultado = await _mediator.Send(new AtualizaTemplateMetaCommand(empresaId));
            return this.ValidateResponse((int)HttpStatusCode.OK, resultado);
        }
    }
}
