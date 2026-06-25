using Microsoft.AspNetCore.Mvc;
using ProjetoMetaMensagem.Dominio.Interfaces.Mediator;
using ProjetoMetaMensagem.Dominio.UseCases.Empresa.AlteraEmpresa;
using ProjetoMetaMensagem.Dominio.UseCases.Empresa.AtualizaWabaId;
using ProjetoMetaMensagem.Dominio.UseCases.Empresa.CriaEmpresa;
using ProjetoMetaMensagem.Dominio.UseCases.Empresa.DeletaEmpresa;
using ProjetoMetaMensagem.Dominio.UseCases.Empresa.ObtemEmpresa;
using ProjetoMetaMensagem.WebAPI.Common;
using System.Net;

namespace ProjetoMetaMensagem.WebAPI.Controllers.Empresa
{
    [ApiController]
    public class EmpresasController : Controller
    {
        private readonly IMediator _mediator;

        public EmpresasController(IMediator mediator) => _mediator = mediator;

        [HttpPost("api/v2/empresa/incluir")]
        public async Task<IActionResult> Incluir([FromBody] CriaEmpresaCommand command)
        {
            var resultado = await _mediator.Send(command);
            return this.ValidateResponse(resultado != null ? (int)HttpStatusCode.Created : (int)HttpStatusCode.BadRequest, resultado);
        }

        [HttpPut("api/v2/empresa/alterar")]
        public async Task<IActionResult> Alterar([FromBody] AlteraEmpresaCommand command)
        {
            var resultado = await _mediator.Send(command);
            return this.ValidateResponse((int)HttpStatusCode.OK, resultado);
        }

        [HttpDelete("api/v2/empresa/excluir/{id}")]
        public async Task<IActionResult> Excluir(string id)
        {
            var resultado = await _mediator.Send(new DeletaEmpresaCommand { IdEmpresa = id });
            return this.ValidateResponse((int)HttpStatusCode.OK, resultado);
        }

        //[HttpGet("api/empresa/obter-por-id/{id}")]
        //public async Task<IActionResult> ObterPorId(string id)
        //{
        //    var resultado = await _mediator.Send(new ObterEmpresaPorIdQuery { Id = id });
        //    return this.ValidateResponse(resultado != null ? (int)HttpStatusCode.OK : (int)HttpStatusCode.NotFound, resultado);
        //}

        [HttpGet("api/v2/empresa/obter")]
        public async Task<IActionResult> Obter()
        {
            var resultado = await _mediator.Send(new ObtemEmpresaCommand());
            return this.ValidateResponse((int)HttpStatusCode.OK, resultado);
        }

        [HttpPost("api/v2/empresa/atualizar-waba/{empresaId}")]
        public async Task<IActionResult> AtualizarWabaId([FromRoute] Guid empresaId, [FromBody]string accessToken)
        {
            var resultado = await _mediator.Send(new AtualizaWabaIdCommand(empresaId,accessToken));
            return this.ValidateResponse((int)HttpStatusCode.OK, resultado);
        }
    }
}
