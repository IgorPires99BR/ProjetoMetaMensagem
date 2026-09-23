using Microsoft.AspNetCore.Mvc;
using ProjetoMetaMensagem.Dominio.Help.Error;
using ProjetoMetaMensagem.Dominio.Interfaces.Mediator;
using ProjetoMetaMensagem.Dominio.UseCases.CobrancaCliente.ListaCobrancaCliente;
using ProjetoMetaMensagem.Dominio.UseCases.CobrancaCliente.MarcaCobrancaClientePaga;
using ProjetoMetaMensagem.WebAPI.Common;
using System.Net;

namespace ProjetoMetaMensagem.WebAPI.Controllers.CobrancaCliente
{
    // Cobranca que quem revende a plataforma (ex: Sebrecon) faz ao PROPRIO cliente dela --
    // aberta automaticamente a cada disparo de Template com GeraCobranca = true (ver
    // EnviarMensagemTemplateMetaHandler/...Lote). Sem relacao com /api/cobranca/assinaturas
    // (CobrancasController), que e a assinatura SaaS da propria Contact Solution.
    [ApiController]
    public class CobrancaClienteController : ControllerBase
    {
        private readonly IMediator _mediator;
        private readonly ILogger<CobrancaClienteController> _logger;

        public CobrancaClienteController(IMediator mediator, ILogger<CobrancaClienteController> logger)
        {
            _mediator = mediator;
            _logger = logger;
        }

        // status: PENDENTE, PAGA, VENCIDA (pendente + vencimento no passado) ou CANCELADA.
        // Sem filtro = todas da empresa. E a lista que o futuro job de recorrencia vai consumir
        // com status=VENCIDA. dataInicio/dataFim (yyyy-MM-dd) filtram pela data de envio.
        [HttpGet("api/cobranca-cliente")]
        public async Task<IActionResult> Listar([FromQuery] string? status, [FromQuery] DateTime? dataInicio, [FromQuery] DateTime? dataFim)
        {
            try
            {
                var command = new ListaCobrancaClienteCommand
                {
                    EmpresaIdSolicitante = this.EmpresaDoEscopo(),
                    Status = status,
                    DataInicio = dataInicio,
                    DataFim = dataFim
                };

                var resultado = await _mediator.Send(command);
                return this.ValidateResponse((int)HttpStatusCode.OK, resultado);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { mensagem = TratamentoErro.Tratar(ex, _logger, "CobrancaClienteController.Listar"), tipo = "Servico" });
            }
        }

        // Marcacao manual: por enquanto e o unico jeito confiavel de confirmar pagamento (ver
        // comentario em MarcaCobrancaClientePagaCommand).
        [HttpPatch("api/cobranca-cliente/{id}/marcar-paga")]
        public async Task<IActionResult> MarcarPaga(Guid id, [FromBody] MarcaCobrancaClientePagaCommand command)
        {
            try
            {
                command.CobrancaClienteId = id;
                command.EmpresaIdSolicitante = this.EmpresaDoEscopo();

                var resultado = await _mediator.Send(command);
                return this.ValidateResponse((int)HttpStatusCode.OK, resultado);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { mensagem = TratamentoErro.Tratar(ex, _logger, "CobrancaClienteController.MarcarPaga"), tipo = "Servico" });
            }
        }
    }
}
