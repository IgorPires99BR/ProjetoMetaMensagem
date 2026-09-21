using ProjetoMetaMensagem.Dominio.Help.Error;
using Microsoft.AspNetCore.Mvc;
using ProjetoMetaMensagem.Dominio.Interfaces.Mediator;
using ProjetoMetaMensagem.Dominio.UseCases.Empresa.AlteraEmpresa;
using ProjetoMetaMensagem.Dominio.UseCases.Empresa.AtualizaWabaId;
using ProjetoMetaMensagem.Dominio.UseCases.Empresa.ConectaContaMeta;
using ProjetoMetaMensagem.Dominio.UseCases.Empresa.CriaContaCliente;
using ProjetoMetaMensagem.Dominio.UseCases.Empresa.CriaEmpresa;
using ProjetoMetaMensagem.Dominio.UseCases.Empresa.DeletaEmpresa;
using ProjetoMetaMensagem.Dominio.UseCases.Empresa.ObtemEmpresa;
using ProjetoMetaMensagem.WebAPI.Common;
using System.Net;

namespace ProjetoMetaMensagem.WebAPI.Controllers.Empresa
{
    [ApiController]
    public class EmpresasController : Controller
    {
        private readonly IMediator _mediator;

        private readonly ILogger<EmpresasController> _logger;

        public EmpresasController(IMediator mediator, ILogger<EmpresasController> logger)
        {
            _mediator = mediator;
            _logger = logger;
        }

        [HttpPost("api/v2/empresa/criar-conta-cliente")]
        public async Task<IActionResult> CriarContaCliente([FromBody] CriaContaClienteCommand command)
        {
            try
            {
                // Do token, nunca do corpo: senao qualquer admin de qualquer empresa cliente
                // se declararia conta de operacao e criaria tenants novos.
                command.SolicitanteEhAdminDaPlataforma = this.EhAdminDaPlataforma();

                var resultado = await _mediator.Send(command);
                return this.ValidateResponse(resultado != null ? (int)HttpStatusCode.Created : (int)HttpStatusCode.BadRequest, resultado);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { mensagem = TratamentoErro.Tratar(ex, _logger, "EmpresasController.CriarContaCliente"), tipo = "Servico" });
            }
        }

        [HttpPost("api/v2/empresa/incluir")]
        public async Task<IActionResult> Incluir([FromBody] CriaEmpresaCommand command)
        {
            try
            {
                var resultado = await _mediator.Send(command);
                return this.ValidateResponse(resultado != null ? (int)HttpStatusCode.Created : (int)HttpStatusCode.BadRequest, resultado);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { mensagem = TratamentoErro.Tratar(ex, _logger, "EmpresasController.Incluir"), tipo = "Servico" });
            }
        }

        [HttpPut("api/v2/empresa/alterar")]
        public async Task<IActionResult> Alterar([FromBody] AlteraEmpresaCommand command)
        {
            try
            {
                var resultado = await _mediator.Send(command);
                return this.ValidateResponse((int)HttpStatusCode.OK, resultado);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { mensagem = TratamentoErro.Tratar(ex, _logger, "EmpresasController.Alterar"), tipo = "Servico" });
            }
        }

        [HttpDelete("api/v2/empresa/excluir/{id}")]
        public async Task<IActionResult> Excluir(string id)
        {
            try
            {
                var resultado = await _mediator.Send(new DeletaEmpresaCommand { IdEmpresa = id });
                return this.ValidateResponse((int)HttpStatusCode.OK, resultado);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { mensagem = TratamentoErro.Tratar(ex, _logger, "EmpresasController.Excluir"), tipo = "Servico" });
            }
        }

        //[HttpGet("api/empresa/obter-por-id/{id}")]
        //public async Task<IActionResult> ObterPorId(string id)
        //{
        //    var resultado = await _mediator.Send(new ObterEmpresaPorIdQuery { Id = id });
        //    return this.ValidateResponse(resultado != null ? (int)HttpStatusCode.OK : (int)HttpStatusCode.NotFound, resultado);
        //}

        [HttpGet("api/v2/empresa/obter")]
        public async Task<IActionResult> Obter()
        {
            try
            {
                // Escopo vem do token, nunca do cliente.
                var claimEmpresa = User.FindFirst("empresaId")?.Value;
                // Listar TODAS as empresas (com o MetaAccessToken de cada uma) e privilegio de
                // plataforma. O admin do cliente abre a mesma tela, mas so com a empresa dele.
                var comando = new ObtemEmpresaCommand
                {
                    SolicitanteEhAdmin = this.EhAdminDaPlataforma(),
                    EmpresaIdSolicitante = Guid.TryParse(claimEmpresa, out var idEmpresa) ? idEmpresa : null
                };

                var resultado = await _mediator.Send(comando);
                return this.ValidateResponse((int)HttpStatusCode.OK, resultado);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { mensagem = TratamentoErro.Tratar(ex, _logger, "EmpresasController.Obter"), tipo = "Servico" });
            }
        }

        // Embedded Signup a nivel de Empresa: provisiona a empresa (cliente novo, cadastrado
        // via "Cadastro rápido") com WabaId/PhoneNumberId/AccessToken proprios, pra ela deixar
        // de compartilhar o numero da Contact Solution. So a conta de plataforma pode chamar --
        // nao expor isso pra admin de empresa cliente, que nao tem "code" nenhum pra trocar
        // (o botao nem aparece pra ele no front) e nao faz sentido reconectar a propria conta.
        [HttpPost("api/v2/empresa/conectar-meta")]
        public async Task<IActionResult> ConectarMeta([FromBody] ConectaContaMetaCommand command)
        {
            try
            {
                command.SolicitanteEhAdminDaPlataforma = this.EhAdminDaPlataforma();
                if (!command.SolicitanteEhAdminDaPlataforma)
                {
                    return StatusCode(403, new { mensagem = "Apenas a conta de plataforma pode conectar uma empresa direto à Meta.", tipo = "Negocio" });
                }

                var resultado = await _mediator.Send(command);
                return this.ValidateResponse((int)HttpStatusCode.OK, resultado);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { mensagem = TratamentoErro.Tratar(ex, _logger, "EmpresasController.ConectarMeta"), tipo = "Servico" });
            }
        }

        [HttpPost("api/v2/empresa/atualizar-waba/{empresaId}")]
        public async Task<IActionResult> AtualizarWabaId([FromRoute] Guid empresaId, [FromBody]string accessToken)
        {
            try
            {
                var resultado = await _mediator.Send(new AtualizaWabaIdCommand(empresaId,accessToken));
                return this.ValidateResponse((int)HttpStatusCode.OK, resultado);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { mensagem = TratamentoErro.Tratar(ex, _logger, "EmpresasController.AtualizarWabaId"), tipo = "Servico" });
            }
        }
    }
}
