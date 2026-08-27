using ProjetoMetaMensagem.Dominio.Help.Error;
using Microsoft.AspNetCore.Mvc;
using ProjetoMetaMensagem.Dominio.Interfaces.Mediator;
using ProjetoMetaMensagem.Dominio.UseCases.Usuario.AlteraUsuario;
using ProjetoMetaMensagem.Dominio.UseCases.Usuario.CriaUsuario;
using ProjetoMetaMensagem.Dominio.UseCases.Usuario.DeletaUsuario;
using ProjetoMetaMensagem.Dominio.UseCases.Usuario.ObtemUsuario;
using ProjetoMetaMensagem.Dominio.UseCases.Usuario.TrocaSenha;
using ProjetoMetaMensagem.WebAPI.Common;
using System.Net;

namespace ProjetoMetaMensagem.WebAPI.Controllers.Usuario
{
    [ApiController]
    public class UsuariosController : Controller
    {
        private readonly IMediator _mediator;
        private readonly ILogger<UsuariosController> _logger;

        public UsuariosController(IMediator mediator, ILogger<UsuariosController> logger)
        {
            _mediator = mediator;
            _logger = logger;
        }

        [HttpPost("api/usuario/incluir")]
        public async Task<IActionResult> Incluir([FromBody] CriaUsuarioCommand command)
        {
            try
            {
                // Escopo vem do token, nunca do corpo (mesmo motivo do Deletar logo abaixo):
                // sem isso, qualquer usuario autenticado podia criar um usuario dentro de OUTRA
                // empresa mandando o empresaId dela no JSON.
                command.EmpresaIdSolicitante = this.EmpresaDoEscopo();
                command.SolicitanteEhAdmin = this.EhAdmin() || this.EhAdminDaPlataforma();

                var resultado = await _mediator.Send(command);
                return this.ValidateResponse(resultado != null ? (int)HttpStatusCode.Created : (int)HttpStatusCode.BadRequest, resultado);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { mensagem = TratamentoErro.Tratar(ex, _logger, "UsuariosController.Incluir"), tipo = "Servico" });
            }
        }

        [HttpPost("api/usuario/trocar-senha")]
        public async Task<IActionResult> TrocarSenha([FromBody] TrocaSenhaCommand command)
        {
            try
            {
                // Quem troca a senha e sempre o dono do token. Se o id viesse do corpo,
                // qualquer usuario logado trocaria a senha de outro so mandando o id dele.
                command.UsuarioIdDoToken = this.UsuarioIdDoEscopo();

                var resultado = await _mediator.Send(command);
                return this.ValidateResponse(resultado != null ? (int)HttpStatusCode.OK : (int)HttpStatusCode.BadRequest, resultado);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { mensagem = TratamentoErro.Tratar(ex, _logger, "UsuariosController.TrocarSenha"), tipo = "Servico" });
            }
        }

        [HttpDelete("api/usuario/excluir/{id}")]
        public async Task<IActionResult> Deletar(Guid id)
        {
            try
            {
                // Escopo vem do token, nunca da rota/corpo: senao o proprio atacante o escolheria.
                DeletaUsuarioCommand command = new DeletaUsuarioCommand(id)
                {
                    EmpresaIdSolicitante = this.EmpresaDoEscopo()
                };

                var resultado = await _mediator.Send(command);
                return this.ValidateResponse(resultado != null ? (int)HttpStatusCode.Created : (int)HttpStatusCode.BadRequest, resultado);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { mensagem = TratamentoErro.Tratar(ex, _logger, "UsuariosController.Deletar"), tipo = "Servico" });
            }
        }

        [HttpPut("api/usuario/alterar")]
        public async Task<IActionResult> Alterar([FromBody] AlteraUsuarioCommand command)
        {
            try
            {
                // Escopo vem do token, nunca do corpo: senao o proprio atacante o escolheria.
                command.EmpresaIdSolicitante = this.EmpresaDoEscopo();
                command.SolicitanteEhAdmin = this.EhAdmin() || this.EhAdminDaPlataforma();

                var resultado = await _mediator.Send(command);
                return this.ValidateResponse(resultado != null ? (int)HttpStatusCode.Created : (int)HttpStatusCode.BadRequest, resultado);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { mensagem = TratamentoErro.Tratar(ex, _logger, "UsuariosController.Alterar"), tipo = "Servico" });
            }
        }

        [HttpGet("api/usuario/obter-por-empresa/{idEmpresa}")]
        public async Task<IActionResult> ObterPorId(Guid idEmpresa)
        {
            try
            {
                var resultado = await _mediator.Send(new ObtemUsuarioCommand(idEmpresa));
                return this.ValidateResponse(resultado != null ? (int)HttpStatusCode.OK : (int)HttpStatusCode.NotFound, resultado);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { mensagem = TratamentoErro.Tratar(ex, _logger, "UsuariosController.ObterPorId"), tipo = "Servico" });
            }
        }
    }
}
