using ProjetoMetaMensagem.Dominio.Help.Error;
using Microsoft.AspNetCore.Mvc;
using ProjetoMetaMensagem.Dominio.Interfaces.Mediator;
using ProjetoMetaMensagem.Dominio.UseCases.Perfil.AlteraPerfil;
using ProjetoMetaMensagem.Dominio.UseCases.Perfil.CriaPerfil;
using ProjetoMetaMensagem.Dominio.UseCases.Perfil.DeletaPerfil;
using ProjetoMetaMensagem.Dominio.UseCases.Perfil.ObtemPerfil;
using ProjetoMetaMensagem.WebAPI.Common;
using System;
using System.Net;
using System.Threading.Tasks;

namespace ProjetoMetaMensagem.WebAPI.Controllers.Perfil
{
    [ApiController]
    public class PerfisController : Controller
    {
        private readonly IMediator _mediator;
        private readonly ILogger<PerfisController> _logger;

        public PerfisController(IMediator mediator, ILogger<PerfisController> logger)
        {
            _mediator = mediator;
            _logger = logger;
        }

        // Escrita e exclusiva da conta de plataforma (a tela Perfis de Acesso so aparece pra
        // ela); a leitura (obter-por-empresa) fica liberada porque o select de Usuarios usa,
        // ja recortada pela empresa do token.
        private IActionResult NegaGestaoDePerfis() =>
            StatusCode(403, new { mensagem = "Apenas a conta de plataforma pode gerenciar perfis de acesso.", tipo = "Negocio" });

        [HttpPost("api/perfil/incluir")]
        public async Task<IActionResult> Incluir([FromBody] CriaPerfilCommand command)
        {
            try
            {
                if (!this.EhAdminDaPlataforma()) return NegaGestaoDePerfis();

                command.EmpresaIdSolicitante = this.EmpresaDoEscopo();

                var resultado = await _mediator.Send(command);
                return this.ValidateResponse(resultado != null ? (int)HttpStatusCode.Created : (int)HttpStatusCode.BadRequest, resultado);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { mensagem = TratamentoErro.Tratar(ex, _logger, "PerfisController.Incluir"), tipo = "Servico" });
            }
        }

        [HttpPut("api/perfil/alterar")]
        public async Task<IActionResult> Alterar([FromBody] AlteraPerfilCommand command)
        {
            try
            {
                if (!this.EhAdminDaPlataforma()) return NegaGestaoDePerfis();

                command.EmpresaIdSolicitante = this.EmpresaDoEscopo();

                var resultado = await _mediator.Send(command);
                return this.ValidateResponse(resultado != null ? (int)HttpStatusCode.Created : (int)HttpStatusCode.BadRequest, resultado);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { mensagem = TratamentoErro.Tratar(ex, _logger, "PerfisController.Alterar"), tipo = "Servico" });
            }
        }

        [HttpGet("api/perfil/obter-por-empresa")]
        public async Task<IActionResult> ObterPorEmpresa()
        {
            try
            {
                var resultado = await _mediator.Send(new ObtemPerfilCommand(this.EmpresaDoEscopo()));
                return this.ValidateResponse((int)HttpStatusCode.OK, resultado);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { mensagem = TratamentoErro.Tratar(ex, _logger, "PerfisController.ObterPorEmpresa"), tipo = "Servico" });
            }
        }

        [HttpDelete("api/perfil/excluir/{id}")]
        public async Task<IActionResult> Excluir(Guid id)
        {
            try
            {
                if (!this.EhAdminDaPlataforma()) return NegaGestaoDePerfis();

                var resultado = await _mediator.Send(new DeletaPerfilCommand
                {
                    Id = id,
                    EmpresaIdSolicitante = this.EmpresaDoEscopo()
                });
                return this.ValidateResponse((int)HttpStatusCode.OK, resultado);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { mensagem = TratamentoErro.Tratar(ex, _logger, "PerfisController.Excluir"), tipo = "Servico" });
            }
        }
    }
}
