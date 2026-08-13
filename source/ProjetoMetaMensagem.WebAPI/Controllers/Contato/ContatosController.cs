using ProjetoMetaMensagem.Dominio.Help.Error;
using Microsoft.AspNetCore.Mvc;
using ProjetoMetaMensagem.Dominio.Interfaces.Mediator;
using ProjetoMetaMensagem.Dominio.UseCases.Contato.AlteraContato;
using ProjetoMetaMensagem.Dominio.UseCases.Contato.CriaContato;
using ProjetoMetaMensagem.Dominio.UseCases.Contato.CriaContatoEmLote;
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
        private readonly ILogger<ContatosController> _logger;

        public ContatosController(IMediator mediator, ILogger<ContatosController> logger)
        {
            _mediator = mediator;
            _logger = logger;
        }

        [HttpPost("api/contato/incluir")]
        public async Task<IActionResult> Incluir([FromBody] CriaContatoCommand command)
        {
            try
            {
                // Escopo vem do token, nunca do corpo: senao o proprio atacante o escolheria.
                command.EmpresaIdSolicitante = this.EmpresaDoEscopo();

                var resultado = await _mediator.Send(command);
                return this.ValidateResponse(resultado != null ? (int)HttpStatusCode.Created : (int)HttpStatusCode.BadRequest, resultado);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { mensagem = TratamentoErro.Tratar(ex, _logger, "ContatosController.Incluir"), tipo = "Servico" });
            }
        }

        [HttpPost("api/contato/incluir-em-lote")]
        public async Task<IActionResult> IncluirEmLote([FromBody] CriaContatoEmLoteCommand command)
        {
            try
            {
                var resultado = await _mediator.Send(command);
                return this.ValidateResponse(resultado != null ? (int)HttpStatusCode.Created : (int)HttpStatusCode.BadRequest, resultado);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { mensagem = TratamentoErro.Tratar(ex, _logger, "ContatosController.IncluirEmLote"), tipo = "Servico" });
            }
        }

        [HttpPut("api/contato/alterar")]
        public async Task<IActionResult> Alterar([FromBody] AlteraContatoCommand command)
        {
            try
            {
                // Escopo vem do token, nunca do corpo: senao o proprio atacante o escolheria.
                command.EmpresaIdSolicitante = this.EmpresaDoEscopo();

                var resultado = await _mediator.Send(command);
                return this.ValidateResponse(resultado != null ? (int)HttpStatusCode.Created : (int)HttpStatusCode.BadRequest, resultado);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { mensagem = TratamentoErro.Tratar(ex, _logger, "ContatosController.Alterar"), tipo = "Servico" });
            }
        }

        // Rota mantem "{usuarioId}" por compatibilidade com o front (Contatos e Disparador
        // chamam esta mesma rota passando o usuario logado), mas o valor nao e mais usado pra
        // filtrar: a lista e por empresa agora (ver ObtemContatoHandler), com o escopo vindo do
        // token -- nunca da rota, senao o proprio atacante escolheria a empresa.
        [HttpGet("api/contato/obter-por-usuario/{usuarioId}")]
        public async Task<IActionResult> ObterPorUsuario(Guid usuarioId)
        {
            try
            {
                var resultado = await _mediator.Send(new ObtemContatoCommand(this.EmpresaDoEscopo()));
                return this.ValidateResponse((int)HttpStatusCode.OK, resultado);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { mensagem = TratamentoErro.Tratar(ex, _logger, "ContatosController.ObterPorUsuario"), tipo = "Servico" });
            }
        }

        [HttpDelete("api/contato/excluir/{id}")]
        public async Task<IActionResult> Excluir(string id)
        {
            try
            {
                var resultado = await _mediator.Send(new DeletaContatoCommand
                {
                    Id = id,
                    EmpresaIdSolicitante = this.EmpresaDoEscopo()
                });
                return this.ValidateResponse((int)HttpStatusCode.OK, resultado);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { mensagem = TratamentoErro.Tratar(ex, _logger, "ContatosController.Excluir"), tipo = "Servico" });
            }
        }
    }
}
