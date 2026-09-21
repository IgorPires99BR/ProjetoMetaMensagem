using ProjetoMetaMensagem.Dominio.Help.Error;
using Microsoft.AspNetCore.Mvc;
using ProjetoMetaMensagem.Dominio.Interfaces.Mediator;
using ProjetoMetaMensagem.Dominio.UseCases.Parametro.AlteraParametro;
using ProjetoMetaMensagem.Dominio.UseCases.Parametro.CriaParametro;
using ProjetoMetaMensagem.Dominio.UseCases.Parametro.DeletaParametro;
using ProjetoMetaMensagem.Dominio.UseCases.Parametro.ObtemParametro;
using ProjetoMetaMensagem.WebAPI.Common;
using System;
using System.Net;
using System.Threading.Tasks;

namespace ProjetoMetaMensagem.WebAPI.Controllers.Parametro
{
    [ApiController]
    public class ParametrosController : Controller
    {
        private readonly IMediator _mediator;
        private readonly ILogger<ParametrosController> _logger;

        public ParametrosController(IMediator mediator, ILogger<ParametrosController> logger)
        {
            _mediator = mediator;
            _logger = logger;
        }

        // EmpresaId no corpo e conferido pelo EmpresaAccessFilter contra o token (so a conta de
        // plataforma escolhe outra empresa) -- por isso nao ha recorte extra aqui.
        [HttpPost("api/parametro/incluir")]
        public async Task<IActionResult> Incluir([FromBody] CriaParametroCommand command)
        {
            try
            {
                var resultado = await _mediator.Send(command);
                return this.ValidateResponse(resultado != null ? (int)HttpStatusCode.Created : (int)HttpStatusCode.BadRequest, resultado);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { mensagem = TratamentoErro.Tratar(ex, _logger, "ParametrosController.Incluir"), tipo = "Servico" });
            }
        }

        [HttpPut("api/parametro/alterar")]
        public async Task<IActionResult> Alterar([FromBody] AlteraParametroCommand command)
        {
            try
            {
                // Escopo vem do token, nunca do corpo (o Id sozinho nao diz de que empresa e).
                command.EmpresaIdSolicitante = this.EmpresaDoEscopo();

                var resultado = await _mediator.Send(command);
                return this.ValidateResponse(resultado != null ? (int)HttpStatusCode.OK : (int)HttpStatusCode.BadRequest, resultado);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { mensagem = TratamentoErro.Tratar(ex, _logger, "ParametrosController.Alterar"), tipo = "Servico" });
            }
        }

        [HttpGet("api/parametro/obter-por-empresa/{empresaId}")]
        public async Task<IActionResult> ObterPorEmpresa(Guid empresaId)
        {
            try
            {
                var resultado = await _mediator.Send(new ObtemParametroCommand(empresaId));
                return this.ValidateResponse((int)HttpStatusCode.OK, resultado);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { mensagem = TratamentoErro.Tratar(ex, _logger, "ParametrosController.ObterPorEmpresa"), tipo = "Servico" });
            }
        }

        [HttpDelete("api/parametro/excluir/{id}")]
        public async Task<IActionResult> Excluir(Guid id)
        {
            try
            {
                var resultado = await _mediator.Send(new DeletaParametroCommand
                {
                    Id = id,
                    EmpresaIdSolicitante = this.EmpresaDoEscopo()
                });
                return this.ValidateResponse((int)HttpStatusCode.OK, resultado);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { mensagem = TratamentoErro.Tratar(ex, _logger, "ParametrosController.Excluir"), tipo = "Servico" });
            }
        }
    }
}
