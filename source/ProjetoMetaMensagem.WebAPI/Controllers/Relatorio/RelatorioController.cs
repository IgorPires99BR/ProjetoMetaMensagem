using ProjetoMetaMensagem.Dominio.Help.Error;
using Microsoft.AspNetCore.Mvc;
using ProjetoMetaMensagem.Dominio.Interfaces.Mediator;
using ProjetoMetaMensagem.Dominio.UseCases.Relatorio.ListaRelatorioMensagens;
using ProjetoMetaMensagem.Dominio.UseCases.Relatorio.ObtemRelatorioFinanceiro;
using ProjetoMetaMensagem.Dominio.UseCases.Relatorio.ObtemRelatorioEngajamento;
using ProjetoMetaMensagem.Dominio.UseCases.Relatorio.ObtemPrecoCategoria;
using ProjetoMetaMensagem.Dominio.UseCases.Relatorio.AtualizaPrecoCategoria;
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

        // GET /api/relatorio/financeiro?dataInicio=&dataFim=
        // Gasto estimado por cliente/mes. So admin (escopo vem da claim do token, nao do cliente).
        [HttpGet("api/relatorio/financeiro")]
        public async Task<IActionResult> Financeiro(DateTime? dataInicio, DateTime? dataFim)
        {
            try
            {
                // Gasto/engajamento cruzando empresas e dado de plataforma, nao de admin de
                // cliente -- mesmo cuidado do commit 30620f3 (EmpresasController.Obter).
                var ehAdmin = this.EhAdminDaPlataforma();

                var command = new ObtemRelatorioFinanceiroCommand
                {
                    SolicitanteEhAdmin = ehAdmin,
                    DataInicio = dataInicio,
                    DataFim = dataFim
                };

                var resultado = await _mediator.Send(command);
                return this.ValidateResponse((int)HttpStatusCode.OK, resultado);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { mensagem = TratamentoErro.Tratar(ex, _logger, "RelatorioController.Financeiro"), tipo = "Servico" });
            }
        }

        // GET /api/relatorio/engajamento?empresaId=&dataInicio=&dataFim=
        // Funil de enviados/visualizaram/responderam por cliente. Admin ve todos (ou filtra por
        // empresaId); operador comum so enxerga a propria empresa, ignorando o empresaId da query.
        [HttpGet("api/relatorio/engajamento")]
        public async Task<IActionResult> Engajamento(Guid? empresaId, DateTime? dataInicio, DateTime? dataFim)
        {
            try
            {
                var claimEmpresa = User.FindFirst("empresaId")?.Value;
                var ehAdmin = this.EhAdminDaPlataforma();

                var command = new ObtemRelatorioEngajamentoCommand
                {
                    SolicitanteEhAdmin = ehAdmin,
                    EmpresaIdSolicitante = Guid.TryParse(claimEmpresa, out var idEmpresa) ? idEmpresa : null,
                    EmpresaIdFiltro = empresaId,
                    DataInicio = dataInicio,
                    DataFim = dataFim
                };

                var resultado = await _mediator.Send(command);
                return this.ValidateResponse((int)HttpStatusCode.OK, resultado);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { mensagem = TratamentoErro.Tratar(ex, _logger, "RelatorioController.Engajamento"), tipo = "Servico" });
            }
        }

        // GET /api/relatorio/precos-categoria
        [HttpGet("api/relatorio/precos-categoria")]
        public async Task<IActionResult> PrecosCategoria()
        {
            try
            {
                var ehAdmin = this.EhAdminDaPlataforma();

                var resultado = await _mediator.Send(new ObtemPrecoCategoriaCommand { SolicitanteEhAdmin = ehAdmin });
                return this.ValidateResponse((int)HttpStatusCode.OK, resultado);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { mensagem = TratamentoErro.Tratar(ex, _logger, "RelatorioController.PrecosCategoria"), tipo = "Servico" });
            }
        }

        // PUT /api/relatorio/precos-categoria/{categoria}
        [HttpPut("api/relatorio/precos-categoria/{categoria}")]
        public async Task<IActionResult> AtualizaPrecoCategoria(string categoria, [FromBody] AtualizaPrecoCategoriaRequest body)
        {
            try
            {
                var ehAdmin = this.EhAdminDaPlataforma();

                var command = new AtualizaPrecoCategoriaCommand
                {
                    SolicitanteEhAdmin = ehAdmin,
                    Categoria = categoria,
                    PrecoUnitario = body.PrecoUnitario
                };

                var resultado = await _mediator.Send(command);
                return this.ValidateResponse((int)HttpStatusCode.OK, resultado);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { mensagem = TratamentoErro.Tratar(ex, _logger, "RelatorioController.AtualizaPrecoCategoria"), tipo = "Servico" });
            }
        }
    }

    public class AtualizaPrecoCategoriaRequest
    {
        public decimal PrecoUnitario { get; set; }
    }
}
