using ProjetoMetaMensagem.Dominio.Help.Error;
using Microsoft.AspNetCore.Mvc;
using ProjetoMetaMensagem.Dominio.Interfaces.Mediator;
using ProjetoMetaMensagem.Dominio.UseCases.Produto.CriaProduto;
using ProjetoMetaMensagem.Dominio.UseCases.Produto.AlteraProduto;
using ProjetoMetaMensagem.Dominio.UseCases.Produto.DeletaProduto;
using ProjetoMetaMensagem.Dominio.UseCases.Produto.ListaProduto;
using ProjetoMetaMensagem.WebAPI.Common;
using System;
using System.Net;
using System.Threading.Tasks;

namespace ProjetoMetaMensagem.WebAPI.Controllers.Produto
{
    [ApiController]
    public class ProdutosController : Controller
    {
        private readonly IMediator _mediator;

        private readonly ILogger<ProdutosController> _logger;

        public ProdutosController(IMediator mediator, ILogger<ProdutosController> logger)
        {
            _mediator = mediator;
            _logger = logger;
        }

        [HttpPost("api/v2/produto/incluir")]
        public async Task<IActionResult> Incluir([FromBody] CriaProdutoCommand command)
        {
            try
            {
                var resultado = await _mediator.Send(command);
                return this.ValidateResponse(resultado != null ? (int)HttpStatusCode.Created : (int)HttpStatusCode.BadRequest, resultado);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { mensagem = TratamentoErro.Tratar(ex, _logger, "ProdutosController.Incluir"), tipo = "Servico" });
            }
        }

        [HttpPut("api/v2/produto/alterar")]
        public async Task<IActionResult> Alterar([FromBody] AlteraProdutoCommand command)
        {
            try
            {
                // Escopo vem do token, nunca do corpo: senao o proprio atacante o escolheria.
                command.EmpresaIdSolicitante = this.EmpresaDoEscopo();

                var resultado = await _mediator.Send(command);
                return this.ValidateResponse((int)HttpStatusCode.OK, resultado);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { mensagem = TratamentoErro.Tratar(ex, _logger, "ProdutosController.Alterar"), tipo = "Servico" });
            }
        }

        [HttpDelete("api/v2/produto/excluir/{id}")]
        public async Task<IActionResult> Excluir(Guid id)
        {
            try
            {
                var resultado = await _mediator.Send(new DeletaProdutoCommand
                {
                    Id = id,
                    EmpresaIdSolicitante = this.EmpresaDoEscopo()
                });
                return this.ValidateResponse((int)HttpStatusCode.OK, resultado);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { mensagem = TratamentoErro.Tratar(ex, _logger, "ProdutosController.Excluir"), tipo = "Servico" });
            }
        }

        [HttpGet("api/v2/produto/listar/{empresaId}")]
        public async Task<IActionResult> ListarPorEmpresa(Guid empresaId)
        {
            try
            {
                var resultado = await _mediator.Send(new ListaProdutoCommand(empresaId));
                return this.ValidateResponse((int)HttpStatusCode.OK, resultado);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { mensagem = TratamentoErro.Tratar(ex, _logger, "ProdutosController.ListarPorEmpresa"), tipo = "Servico" });
            }
        }
    }
}
