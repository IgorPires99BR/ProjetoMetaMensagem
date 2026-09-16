using ProjetoMetaMensagem.Dominio.Help.Error;
using Microsoft.AspNetCore.Mvc;
using ProjetoMetaMensagem.Dominio.Interfaces.Mediator;
using ProjetoMetaMensagem.Dominio.UseCases.Agendamento.AlteraAgendamento;
using ProjetoMetaMensagem.Dominio.UseCases.Agendamento.CriaAgendamento;
using ProjetoMetaMensagem.Dominio.UseCases.Agendamento.DeletaAgendamento;
using ProjetoMetaMensagem.Dominio.UseCases.Agendamento.ListaAgendamento;
using ProjetoMetaMensagem.Dominio.UseCases.Agendamento.ListaExecucoesAgendamento;
using ProjetoMetaMensagem.Dominio.UseCases.Agendamento.ObtemAgendamento;
using ProjetoMetaMensagem.Dominio.UseCases.Agendamento.PausaRetomaAgendamento;
using ProjetoMetaMensagem.WebAPI.Common;
using System;
using System.Net;
using System.Threading.Tasks;

namespace ProjetoMetaMensagem.WebAPI.Controllers.Agendamento
{
    [ApiController]
    public class AgendamentosController : Controller
    {
        private readonly IMediator _mediator;
        private readonly ILogger<AgendamentosController> _logger;

        public AgendamentosController(IMediator mediator, ILogger<AgendamentosController> logger)
        {
            _mediator = mediator;
            _logger = logger;
        }

        [HttpPost("api/agendamento/incluir")]
        public async Task<IActionResult> Incluir([FromBody] CriaAgendamentoCommand command)
        {
            try
            {
                var resultado = await _mediator.Send(command);
                return this.ValidateResponse(resultado != null ? (int)HttpStatusCode.Created : (int)HttpStatusCode.BadRequest, resultado);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { mensagem = TratamentoErro.Tratar(ex, _logger, "AgendamentosController.Incluir"), tipo = "Servico" });
            }
        }

        [HttpGet("api/agendamento/{id}")]
        public async Task<IActionResult> Obter([FromRoute] Guid id)
        {
            try
            {
                var resultado = await _mediator.Send(new ObtemAgendamentoCommand
                {
                    Id = id,
                    EmpresaIdSolicitante = this.EmpresaDoEscopo()
                });
                return this.ValidateResponse((int)HttpStatusCode.OK, resultado);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { mensagem = TratamentoErro.Tratar(ex, _logger, "AgendamentosController.Obter"), tipo = "Servico" });
            }
        }

        [HttpPut("api/agendamento/alterar")]
        public async Task<IActionResult> Alterar([FromBody] AlteraAgendamentoCommand command)
        {
            try
            {
                command.EmpresaIdSolicitante = this.EmpresaDoEscopo();
                var resultado = await _mediator.Send(command);
                return this.ValidateResponse((int)HttpStatusCode.OK, resultado);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { mensagem = TratamentoErro.Tratar(ex, _logger, "AgendamentosController.Alterar"), tipo = "Servico" });
            }
        }

        [HttpGet("api/agendamento/listar/{empresaId}")]
        public async Task<IActionResult> Listar([FromRoute] Guid empresaId)
        {
            try
            {
                var resultado = await _mediator.Send(new ListaAgendamentoCommand { EmpresaId = empresaId });
                return this.ValidateResponse((int)HttpStatusCode.OK, resultado);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { mensagem = TratamentoErro.Tratar(ex, _logger, "AgendamentosController.Listar"), tipo = "Servico" });
            }
        }

        public class AlterarStatusRequest
        {
            public bool Ativo { get; set; }
        }

        [HttpPut("api/agendamento/{id}/status")]
        public async Task<IActionResult> AlterarStatus([FromRoute] Guid id, [FromBody] AlterarStatusRequest body)
        {
            try
            {
                var resultado = await _mediator.Send(new PausaRetomaAgendamentoCommand
                {
                    Id = id,
                    Ativo = body.Ativo,
                    EmpresaIdSolicitante = this.EmpresaDoEscopo()
                });
                return this.ValidateResponse((int)HttpStatusCode.OK, resultado);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { mensagem = TratamentoErro.Tratar(ex, _logger, "AgendamentosController.AlterarStatus"), tipo = "Servico" });
            }
        }

        [HttpDelete("api/agendamento/{id}")]
        public async Task<IActionResult> Excluir([FromRoute] Guid id)
        {
            try
            {
                var resultado = await _mediator.Send(new DeletaAgendamentoCommand
                {
                    Id = id,
                    EmpresaIdSolicitante = this.EmpresaDoEscopo()
                });
                return this.ValidateResponse((int)HttpStatusCode.OK, resultado);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { mensagem = TratamentoErro.Tratar(ex, _logger, "AgendamentosController.Excluir"), tipo = "Servico" });
            }
        }

        [HttpGet("api/agendamento/{id}/execucoes")]
        public async Task<IActionResult> Execucoes([FromRoute] Guid id)
        {
            try
            {
                var resultado = await _mediator.Send(new ListaExecucoesAgendamentoCommand
                {
                    AgendamentoId = id,
                    EmpresaIdSolicitante = this.EmpresaDoEscopo()
                });
                return this.ValidateResponse((int)HttpStatusCode.OK, resultado);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { mensagem = TratamentoErro.Tratar(ex, _logger, "AgendamentosController.Execucoes"), tipo = "Servico" });
            }
        }
    }
}
