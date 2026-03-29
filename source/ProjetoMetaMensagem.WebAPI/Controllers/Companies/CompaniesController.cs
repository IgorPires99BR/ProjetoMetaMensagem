using Microsoft.AspNetCore.Mvc;
using ProjetoMetaMensagem.Dominio.Interfaces.Mediator;
using ProjetoMetaMensagem.Dominio.UseCases.Admin.Companies.AlteraEmpresa;
using ProjetoMetaMensagem.Dominio.UseCases.Admin.Companies.CriaEmpresa;
using ProjetoMetaMensagem.Dominio.UseCases.Admin.Companies.DeletaEmpresa;
using ProjetoMetaMensagem.Dominio.UseCases.Admin.Companies.ObtemEmpresa;
using ProjetoMetaMensagem.Dominio.UseCases.Messages.EnviarMensagemMeta;

namespace ProjetoMetaMensagem.WebAPI.Controllers.Companies
{
    [ApiController]
    [Route("api/admin/[controller]")]
    public class CompaniesController : Controller
    {
        private readonly IMediator _mediator;

        public CompaniesController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpDelete("{company_id}")]
        public async Task<IActionResult> Deletar([FromRoute] int company_id)
        {

            var resultado = await _mediator.Send(new DeletaEmpresaCommand(company_id));

            if (resultado != null)
                return Ok(resultado);

            return BadRequest(resultado);
        }

        [HttpPut("{company_id}")]
        public async Task<IActionResult> Alterar([FromRoute] int company_id, AlteraEmpresaCommand command)
        {
            command.Id = company_id;
            var resultado = await _mediator.Send(command);

            if (resultado != null)
                return Ok(resultado);

            return BadRequest(resultado);
        }

        [HttpGet]
        public async Task<IActionResult> Obtem()
        {
            ObtemEmpresaCommand command = new ObtemEmpresaCommand();
            var resultado = await _mediator.Send(command);

            if (resultado != null)
                return Ok(resultado);

            return BadRequest(resultado);
        }

        [HttpPost]
        public async Task<IActionResult> Incluir(CriaEmpresaCommand command)
        {
            var resultado = await _mediator.Send(command);

            if (resultado != null)
                return Ok(resultado);

            return BadRequest(resultado);
        }
    }
}
