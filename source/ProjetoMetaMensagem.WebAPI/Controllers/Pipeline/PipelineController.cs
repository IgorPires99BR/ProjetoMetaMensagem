using ProjetoMetaMensagem.Dominio.Help.Error;
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
using ProjetoMetaMensagem.WebAPI.Common;
using System.Net;

namespace ProjetoMetaMensagem.WebAPI.Controllers.Pipeline
{
    [ApiController]
    [Route("api/pipeline")]
    public class PipelineController : Controller
    {
        private readonly IMediator _mediator;
        private readonly ILogger<PipelineController> _logger;

        public PipelineController(IMediator mediator, ILogger<PipelineController> logger)
        {
            _mediator = mediator;
            _logger = logger;
        }

        [HttpPost("criar")]
        public async Task<IActionResult> Criar([FromBody] CriaPipelineCommand cmd)
        {
            try
            {
                var resultado = await _mediator.Send(cmd);
                return this.ValidateResponse((int)HttpStatusCode.Created, resultado);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { mensagem = TratamentoErro.Tratar(ex, _logger, "PipelineController.Criar"), tipo = "Servico" });
            }
        }

        [HttpGet("listar/{empresaId}")]
        public async Task<IActionResult> Listar(Guid empresaId)
        {
            try
            {
                var resultado = await _mediator.Send(new ListaPipelineCommand(empresaId));
                return this.ValidateResponse((int)HttpStatusCode.OK, resultado);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { mensagem = TratamentoErro.Tratar(ex, _logger, "PipelineController.Listar"), tipo = "Servico" });
            }
        }

        [HttpGet("obter/{pipelineId}/{empresaId}")]
        public async Task<IActionResult> ObterComEtapas(Guid pipelineId, Guid empresaId)
        {
            try
            {
                var resultado = await _mediator.Send(new ObtemPipelineComEtapasCommand(pipelineId, empresaId));
                return this.ValidateResponse((int)HttpStatusCode.OK, resultado);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { mensagem = TratamentoErro.Tratar(ex, _logger, "PipelineController.ObterComEtapas"), tipo = "Servico" });
            }
        }

        [HttpPut("alterar")]
        public async Task<IActionResult> Alterar([FromBody] AlteraPipelineCommand cmd)
        {
            try
            {
                // Escopo vem do token, nunca do corpo/rota: senao o proprio atacante o escolheria.
                cmd.EmpresaIdSolicitante = this.EmpresaDoEscopo();

                var resultado = await _mediator.Send(cmd);
                return this.ValidateResponse((int)HttpStatusCode.OK, resultado);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { mensagem = TratamentoErro.Tratar(ex, _logger, "PipelineController.Alterar"), tipo = "Servico" });
            }
        }

        [HttpDelete("excluir/{id}")]
        public async Task<IActionResult> Excluir(Guid id)
        {
            try
            {
                var resultado = await _mediator.Send(new DeletaPipelineCommand(id)
                {
                    EmpresaIdSolicitante = this.EmpresaDoEscopo()
                });
                return this.ValidateResponse((int)HttpStatusCode.OK, resultado);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { mensagem = TratamentoErro.Tratar(ex, _logger, "PipelineController.Excluir"), tipo = "Servico" });
            }
        }

        // Etapas
        [HttpPost("etapa/criar")]
        public async Task<IActionResult> CriarEtapa([FromBody] CriaEtapaCommand cmd)
        {
            try
            {
                var resultado = await _mediator.Send(cmd);
                return this.ValidateResponse((int)HttpStatusCode.Created, resultado);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { mensagem = TratamentoErro.Tratar(ex, _logger, "PipelineController.CriarEtapa"), tipo = "Servico" });
            }
        }

        [HttpGet("etapa/listar/{pipelineId}")]
        public async Task<IActionResult> ListarEtapas(Guid pipelineId)
        {
            try
            {
                var resultado = await _mediator.Send(new ListaEtapaCommand(pipelineId));
                return this.ValidateResponse((int)HttpStatusCode.OK, resultado);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { mensagem = TratamentoErro.Tratar(ex, _logger, "PipelineController.ListarEtapas"), tipo = "Servico" });
            }
        }

        [HttpPut("etapa/alterar")]
        public async Task<IActionResult> AlterarEtapa([FromBody] AlteraEtapaCommand cmd)
        {
            try
            {
                cmd.EmpresaIdSolicitante = this.EmpresaDoEscopo();

                var resultado = await _mediator.Send(cmd);
                return this.ValidateResponse((int)HttpStatusCode.OK, resultado);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { mensagem = TratamentoErro.Tratar(ex, _logger, "PipelineController.AlterarEtapa"), tipo = "Servico" });
            }
        }

        [HttpDelete("etapa/excluir/{id}")]
        public async Task<IActionResult> ExcluirEtapa(Guid id)
        {
            try
            {
                var resultado = await _mediator.Send(new DeletaEtapaCommand(id)
                {
                    EmpresaIdSolicitante = this.EmpresaDoEscopo()
                });
                return this.ValidateResponse((int)HttpStatusCode.OK, resultado);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { mensagem = TratamentoErro.Tratar(ex, _logger, "PipelineController.ExcluirEtapa"), tipo = "Servico" });
            }
        }

        // Leads
        [HttpPost("lead/adicionar")]
        public async Task<IActionResult> AdicionarLead([FromBody] AdicionarLeadCommand cmd)
        {
            try
            {
                var resultado = await _mediator.Send(cmd);
                return this.ValidateResponse((int)HttpStatusCode.Created, resultado);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { mensagem = TratamentoErro.Tratar(ex, _logger, "PipelineController.AdicionarLead"), tipo = "Servico" });
            }
        }

        [HttpPut("lead/mover")]
        public async Task<IActionResult> MoverLead([FromBody] MoverLeadCommand cmd)
        {
            try
            {
                cmd.EmpresaIdSolicitante = this.EmpresaDoEscopo();

                var resultado = await _mediator.Send(cmd);
                return this.ValidateResponse((int)HttpStatusCode.OK, resultado);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { mensagem = TratamentoErro.Tratar(ex, _logger, "PipelineController.MoverLead"), tipo = "Servico" });
            }
        }

        [HttpDelete("lead/remover/{leadId}")]
        public async Task<IActionResult> RemoverLead(Guid leadId)
        {
            try
            {
                var resultado = await _mediator.Send(new RemoverLeadCommand(leadId)
                {
                    EmpresaIdSolicitante = this.EmpresaDoEscopo()
                });
                return this.ValidateResponse((int)HttpStatusCode.OK, resultado);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { mensagem = TratamentoErro.Tratar(ex, _logger, "PipelineController.RemoverLead"), tipo = "Servico" });
            }
        }
    }
}
