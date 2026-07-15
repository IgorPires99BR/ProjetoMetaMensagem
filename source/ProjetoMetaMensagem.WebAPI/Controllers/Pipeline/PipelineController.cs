using Microsoft.AspNetCore.Mvc;
using ProjetoMetaMensagem.Dominio.Interfaces.Mediator;
using ProjetoMetaMensagem.Dominio.UseCases.LeadPipeline.Mover;
using ProjetoMetaMensagem.Dominio.UseCases.Pipeline.Altera;
using ProjetoMetaMensagem.Dominio.UseCases.Pipeline.Cria;
using ProjetoMetaMensagem.Dominio.UseCases.Pipeline.Deleta;
using ProjetoMetaMensagem.Dominio.UseCases.Pipeline.Lista;
using ProjetoMetaMensagem.Dominio.UseCases.Pipeline.ObtemComEtapas;
using ProjetoMetaMensagem.Dominio.UseCases.PipelineEtapa.Altera;
using ProjetoMetaMensagem.Dominio.UseCases.PipelineEtapa.Cria;
using ProjetoMetaMensagem.Dominio.UseCases.PipelineEtapa.Deleta;
using ProjetoMetaMensagem.Dominio.UseCases.PipelineEtapa.Lista;

namespace ProjetoMetaMensagem.WebAPI.Controllers.Pipeline
{
    [ApiController]
    [Route("api/pipeline")]
    public class PipelineController : Controller
    {
        private readonly IMediator _mediator;
        public PipelineController(IMediator mediator) => _mediator = mediator;

        [HttpPost("criar")]
        public async Task<IActionResult> Criar([FromBody] CriaPipelineCommand cmd)
        {
            try
            {
                var resultado = await _mediator.Send(cmd);
                return StatusCode(201, resultado);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { erro = ex.Message, detalhe = ex.InnerException?.Message });
            }
        }

        [HttpGet("listar/{empresaId}")]
        public async Task<IActionResult> Listar(Guid empresaId)
        {
            var resultado = await _mediator.Send(new ListaPipelineCommand(empresaId));
            return Ok(resultado);
        }

        [HttpGet("obter/{pipelineId}/{empresaId}")]
        public async Task<IActionResult> ObterComEtapas(Guid pipelineId, Guid empresaId)
        {
            var resultado = await _mediator.Send(new ObtemPipelineComEtapasCommand(pipelineId, empresaId));
            return Ok(resultado);
        }

        [HttpPut("alterar")]
        public async Task<IActionResult> Alterar([FromBody] AlteraPipelineCommand cmd)
        {
            var resultado = await _mediator.Send(cmd);
            return Ok(resultado);
        }

        [HttpDelete("excluir/{id}")]
        public async Task<IActionResult> Excluir(Guid id)
        {
            var resultado = await _mediator.Send(new DeletaPipelineCommand(id));
            return Ok(resultado);
        }

        // Etapas
        [HttpPost("etapa/criar")]
        public async Task<IActionResult> CriarEtapa([FromBody] CriaEtapaCommand cmd)
        {
            try
            {
                var resultado = await _mediator.Send(cmd);
                return StatusCode(201, resultado);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { erro = ex.Message, detalhe = ex.InnerException?.Message });
            }
        }

        [HttpGet("etapa/listar/{pipelineId}")]
        public async Task<IActionResult> ListarEtapas(Guid pipelineId)
        {
            var resultado = await _mediator.Send(new ListaEtapaCommand(pipelineId));
            return Ok(resultado);
        }

        [HttpPut("etapa/alterar")]
        public async Task<IActionResult> AlterarEtapa([FromBody] AlteraEtapaCommand cmd)
        {
            var resultado = await _mediator.Send(cmd);
            return Ok(resultado);
        }

        [HttpDelete("etapa/excluir/{id}")]
        public async Task<IActionResult> ExcluirEtapa(Guid id)
        {
            var resultado = await _mediator.Send(new DeletaEtapaCommand(id));
            return Ok(resultado);
        }

        // Leads
        [HttpPost("lead/adicionar")]
        public async Task<IActionResult> AdicionarLead([FromBody] AdicionarLeadCommand cmd)
        {
            try
            {
                var resultado = await _mediator.Send(cmd);
                return StatusCode(201, resultado);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { erro = ex.Message, detalhe = ex.InnerException?.Message });
            }
        }

        [HttpPut("lead/mover")]
        public async Task<IActionResult> MoverLead([FromBody] MoverLeadCommand cmd)
        {
            var resultado = await _mediator.Send(cmd);
            return Ok(resultado);
        }

        [HttpDelete("lead/remover/{leadId}")]
        public async Task<IActionResult> RemoverLead(Guid leadId)
        {
            var resultado = await _mediator.Send(new RemoverLeadCommand(leadId));
            return Ok(resultado);
        }
    }
}
