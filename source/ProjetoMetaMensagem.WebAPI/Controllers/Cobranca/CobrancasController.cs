using Microsoft.AspNetCore.Mvc;
using ProjetoMetaMensagem.Dominio.Help.Error;
using ProjetoMetaMensagem.Dominio.Interfaces.Mediator;
using ProjetoMetaMensagem.Dominio.UseCases.Cobranca.ListaAssinaturas;
using ProjetoMetaMensagem.WebAPI.Common;
using System.Net;

namespace ProjetoMetaMensagem.WebAPI.Controllers.Cobranca
{
    [ApiController]
    public class CobrancasController : ControllerBase
    {
        private readonly IMediator _mediator;
        private readonly ILogger<CobrancasController> _logger;

        public CobrancasController(IMediator mediator, ILogger<CobrancasController> logger)
        {
            _mediator = mediator;
            _logger = logger;
        }

        // O cliente vê a assinatura da empresa dele; a operação da Contact Solution vê todas.
        // O escopo sai do token, nunca da rota -- senão bastaria trocar o id na URL para ler
        // o faturamento de outro cliente.
        [HttpGet("api/cobranca/assinaturas")]
        public async Task<IActionResult> Listar()
        {
            try
            {
                var command = new ListaAssinaturasCommand(this.EmpresaDoEscopo(), this.EhAdminDaPlataforma());
                var resultado = await _mediator.Send(command);

                return this.ValidateResponse((int)HttpStatusCode.OK, resultado);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { mensagem = TratamentoErro.Tratar(ex, _logger, "CobrancasController.Listar"), tipo = "Servico" });
            }
        }
    }
}
