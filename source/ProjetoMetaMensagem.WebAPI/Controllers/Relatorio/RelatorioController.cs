using ProjetoMetaMensagem.Dominio.Help.Error;
using Microsoft.AspNetCore.Mvc;
using ProjetoMetaMensagem.Dominio.Interfaces.Mediator;
using ProjetoMetaMensagem.Dominio.UseCases.Relatorio.ListaRelatorioMensagens;
using ProjetoMetaMensagem.WebAPI.Common;
using System;
using System.Net;
using System.Threading.Tasks;

namespace ProjetoMetaMensagem.WebAPI.Controllers.Relatorio
{
    [ApiController]
    public class RelatorioController : Controller
    {
        private readonly IMediator _mediator;
        private readonly ILogger<RelatorioController> _logger;

        public RelatorioController(IMediator mediator, ILogger<RelatorioController> logger)
        {
            _mediator = mediator;
            _logger = logger;
        }

        // GET /api/relatorio/mensagens/{empresaId}?dataInicio=&dataFim=&pagina=&tamanho=
        [HttpGet("api/relatorio/mensagens/{empresaId}")]
        public async Task<IActionResult> Mensagens(Guid empresaId, DateTime? dataInicio, DateTime? dataFim, int pagina = 0, int tamanho = 50)
        {
            try
            {
                var command = new ListaRelatorioMensagensCommand
                {
                    EmpresaId = empresaId,
                    DataInicio = dataInicio,
                    DataFim = dataFim,
                    Pagina = pagina,
                    TamanhoPagina = tamanho
                };

                var resultado = await _mediator.Send(command);
                return this.ValidateResponse((int)HttpStatusCode.OK, resultado);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { mensagem = TratamentoErro.Tratar(ex, _logger, "RelatorioController.Mensagens"), tipo = "Servico" });
            }
        }
    }
}
