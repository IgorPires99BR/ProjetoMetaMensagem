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

        public ProdutosController(IMediator mediator) => _mediator = mediator;

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
                return StatusCode(500, new { erro = ex.Message, detalhe = ex.InnerException?.Message });
            }
        }

        [HttpPut("api/v2/produto/alterar")]
        public async Task<IActionResult> Alterar([FromBody] AlteraProdutoCommand command)
        {
            try
            {
                var resultado = await _mediator.Send(command);
                return this.ValidateResponse((int)HttpStatusCode.OK, resultado);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { erro = ex.Message, detalhe = ex.InnerException?.Message });
            }
        }

        [HttpDelete("api/v2/produto/excluir/{id}")]
        public async Task<IActionResult> Excluir(Guid id)
        {
            try
            {
                var resultado = await _mediator.Send(new DeletaProdutoCommand { Id = id });
                return this.ValidateResponse((int)HttpStatusCode.OK, resultado);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { erro = ex.Message, detalhe = ex.InnerException?.Message });
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
                return StatusCode(500, new { erro = ex.Message, detalhe = ex.InnerException?.Message });
            }
        }
    }
}
