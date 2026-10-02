using ProjetoMetaMensagem.Dominio.Help.Error;
using Microsoft.AspNetCore.Mvc;
using ProjetoMetaMensagem.Dominio.Interfaces.Mediator;
using ProjetoMetaMensagem.Dominio.UseCases.Numero.CriaNumero;
using ProjetoMetaMensagem.Dominio.UseCases.Numero.AtualizaNumeroMeta;
using ProjetoMetaMensagem.WebAPI.Common;
using System.Net;
using ProjetoMetaMensagem.Dominio.UseCases.Numero.ListarNumeros;
using ProjetoMetaMensagem.Dominio.UseCases.Numero.IniciaEmbeddedSignup;
using ProjetoMetaMensagem.Dominio.UseCases.Numero.AtivaCoexistencia;
using ProjetoMetaMensagem.Dominio.UseCases.Numero.DeletaNumero;
using ProjetoMetaMensagem.Dominio.UseCases.Numero.ObtemPerfilNumero;
using ProjetoMetaMensagem.Dominio.UseCases.Numero.AlteraPerfilNumero;
using ProjetoMetaMensagem.Dominio.UseCases.Numero.AlteraFotoNumero;
using ProjetoMetaMensagem.Dominio.UseCases.Numero.SolicitaNomeNumero;
using ProjetoMetaMensagem.Dominio.UseCases.Numero.AplicaNomeNumero;

namespace ProjetoMetaMensagem.WebAPI.Controllers.Numero
{
    [ApiController]
    public class NumerosController : Controller
    {
        private readonly IMediator _mediator;
        private readonly ILogger<NumerosController> _logger;

        public NumerosController(IMediator mediator, ILogger<NumerosController> logger)
        {
            _mediator = mediator;
            _logger = logger;
        }

        [HttpPost("api/numero/incluir")]
        public async Task<IActionResult> Incluir([FromBody] CriaNumeroCommand command)
        {
            try
            {
                var resultado = await _mediator.Send(command);
                return this.ValidateResponse((int)HttpStatusCode.Created, resultado);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { mensagem = TratamentoErro.Tratar(ex, _logger, "NumerosController.Incluir"), tipo = "Servico" });
            }
        }

        [HttpGet("api/numero/ListarNumeros/{usuarioId}")]
        public async Task<IActionResult> ListarNumeros(Guid usuarioId)
        {
            try
            {
                var resultado = await _mediator.Send(new ListarNumerosCommand(usuarioId));
                return this.ValidateResponse((int)HttpStatusCode.OK, resultado);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { mensagem = TratamentoErro.Tratar(ex, _logger, "NumerosController.ListarNumeros"), tipo = "Servico" });
            }
        }

        [HttpPost("api/numero/AtualizarNumerosMeta/{usuarioId}")]
        public async Task<IActionResult> AtualizarNumerosMeta(Guid usuarioId, Guid idEmpresa)
        {
            try
            {
                var resultado = await _mediator.Send(new AtualizaNumeroMetaCommand(usuarioId, idEmpresa));
                return this.ValidateResponse((int)HttpStatusCode.OK, resultado);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { mensagem = TratamentoErro.Tratar(ex, _logger, "NumerosController.AtualizarNumerosMeta"), tipo = "Servico" });
            }
        }

        [HttpPost("api/numero/embedded-signup")]
        public async Task<IActionResult> EmbeddedSignup([FromBody] IniciaEmbeddedSignupCommand command)
        {
            try
            {
                var resultado = await _mediator.Send(command);
                return this.ValidateResponse((int)HttpStatusCode.Created, resultado);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { mensagem = TratamentoErro.Tratar(ex, _logger, "NumerosController.EmbeddedSignup"), tipo = "Servico" });
            }
        }

        [HttpPost("api/numero/ativa-coexistencia")]
        public async Task<IActionResult> AtivaCoexistencia(Guid numeroId, Guid idEmpresa, [FromBody] AtivaCoexistenciaRequestBody body)
        {
            try
            {
                var resultado = await _mediator.Send(new AtivaCoexistenciaCommand(numeroId, idEmpresa, body?.Pin));
                return this.ValidateResponse((int)HttpStatusCode.OK, resultado);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { mensagem = TratamentoErro.Tratar(ex, _logger, "NumerosController.AtivaCoexistencia"), tipo = "Servico" });
            }
        }

        [HttpDelete("api/numero/excluir/{id}")]
        public async Task<IActionResult> Excluir(Guid id)
        {
            try
            {
                // Escopo vem do token, nunca da rota/corpo: senao o proprio atacante o escolheria.
                var resultado = await _mediator.Send(new DeletaNumeroCommand
                {
                    Id = id,
                    EmpresaIdSolicitante = this.EmpresaDoEscopo()
                });
                return this.ValidateResponse((int)HttpStatusCode.OK, resultado);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { mensagem = TratamentoErro.Tratar(ex, _logger, "NumerosController.Excluir"), tipo = "Servico" });
            }
        }

        // Perfil do WhatsApp Business: o que o cliente final vê ao abrir o contato do número.
        [HttpGet("api/numero/{id}/perfil")]
        public async Task<IActionResult> ObterPerfil(Guid id)
        {
            try
            {
                var resultado = await _mediator.Send(new ObtemPerfilNumeroCommand
                {
                    NumeroId = id,
                    EmpresaIdSolicitante = this.EmpresaDoEscopo()
                });
                return this.ValidateResponse((int)HttpStatusCode.OK, resultado);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { mensagem = TratamentoErro.Tratar(ex, _logger, "NumerosController.ObterPerfil"), tipo = "Servico" });
            }
        }

        [HttpPut("api/numero/{id}/perfil")]
        public async Task<IActionResult> AlterarPerfil(Guid id, [FromBody] AlteraPerfilNumeroCommand command)
        {
            try
            {
                command.NumeroId = id;
                command.EmpresaIdSolicitante = this.EmpresaDoEscopo();

                var resultado = await _mediator.Send(command);
                return this.ValidateResponse((int)HttpStatusCode.OK, resultado);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { mensagem = TratamentoErro.Tratar(ex, _logger, "NumerosController.AlterarPerfil"), tipo = "Servico" });
            }
        }

        [HttpPost("api/numero/{id}/foto")]
        [RequestSizeLimit(6_000_000)]
        public async Task<IActionResult> AlterarFoto(Guid id, [FromForm] IFormFile arquivo)
        {
            try
            {
                if (arquivo == null || arquivo.Length == 0)
                {
                    return BadRequest(new { mensagem = "Nenhum arquivo enviado.", tipo = "Negocio" });
                }

                using var memoryStream = new MemoryStream();
                await arquivo.CopyToAsync(memoryStream);

                var resultado = await _mediator.Send(new AlteraFotoNumeroCommand
                {
                    NumeroId = id,
                    EmpresaIdSolicitante = this.EmpresaDoEscopo(),
                    Arquivo = memoryStream.ToArray(),
                    MimeType = arquivo.ContentType
                });
                return this.ValidateResponse((int)HttpStatusCode.OK, resultado);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { mensagem = TratamentoErro.Tratar(ex, _logger, "NumerosController.AlterarFoto"), tipo = "Servico" });
            }
        }

        [HttpPost("api/numero/{id}/nome")]
        public async Task<IActionResult> SolicitarNome(Guid id, [FromBody] SolicitaNomeNumeroCommand command)
        {
            try
            {
                command.NumeroId = id;
                command.EmpresaIdSolicitante = this.EmpresaDoEscopo();

                var resultado = await _mediator.Send(command);
                return this.ValidateResponse((int)HttpStatusCode.OK, resultado);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { mensagem = TratamentoErro.Tratar(ex, _logger, "NumerosController.SolicitarNome"), tipo = "Servico" });
            }
        }

        [HttpPost("api/numero/{id}/nome/aplicar")]
        public async Task<IActionResult> AplicarNome(Guid id, [FromBody] AtivaCoexistenciaRequestBody body)
        {
            try
            {
                var resultado = await _mediator.Send(new AplicaNomeNumeroCommand
                {
                    NumeroId = id,
                    EmpresaIdSolicitante = this.EmpresaDoEscopo(),
                    Pin = body?.Pin
                });
                return this.ValidateResponse((int)HttpStatusCode.OK, resultado);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { mensagem = TratamentoErro.Tratar(ex, _logger, "NumerosController.AplicarNome"), tipo = "Servico" });
            }
        }
    }

    // Usado pela coexistência e pela aplicação do novo nome: as duas fazem o /register com PIN.
    public class AtivaCoexistenciaRequestBody
    {
        public string? Pin { get; set; }
    }
}
