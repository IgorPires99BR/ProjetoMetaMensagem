using Microsoft.Extensions.Logging;
using ProjetoMetaMensagem.Dominio.Common;
using ProjetoMetaMensagem.Dominio.Interfaces.Mediator;
using ProjetoMetaMensagem.Dominio.Interfaces;
using System;
using System.Linq;
using System.Threading.Tasks;
using ProjetoMetaMensagem.Dominio.Help.Error;

namespace ProjetoMetaMensagem.Dominio.UseCases.Contato.CriaContato
{
    public class CriaContatoHandler : IRequestHandler<CriaContatoCommand, Response<CriaContatoResult>>
    {
        private readonly IUnitOfWork _unitOfWork;

        private readonly ILogger<CriaContatoHandler> _logger;

        public CriaContatoHandler(IUnitOfWork unitOfWork, ILogger<CriaContatoHandler> logger)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async Task<Response<CriaContatoResult>> Handle(CriaContatoCommand command)
        {
            var response = new Response<CriaContatoResult>();

            try
            {
                // Empresa vem do escopo do token (null so pra conta de plataforma). O
                // EmpresaAccessFilter ja garante que o EmpresaId do corpo bate com o token pra
                // quem nao e admin de plataforma -- aqui so cobre o caso de admin de plataforma
                // mandando um EmpresaId qualquer no corpo, que e esperado.
                if (command.EmpresaIdSolicitante.HasValue)
                {
                    command.EmpresaId = command.EmpresaIdSolicitante.Value;
                }

                _unitOfWork.BeginTransaction();
                var validator = new CriaContatoValidator();
                var validateResult = validator.Validate(command);

                if (!validateResult.IsValid)
                {
                    response.AddErros(validateResult.Errors.ToCustomValidationFailure());
                    return response;
                }

                // UsuarioId precisa ser da mesma empresa do contato -- nao trava a operacao (e
                // so metadado de "quem cadastrou"), mas evita gravar um dono inconsistente.
                var usuario = await _unitOfWork.Usuario.ObterPorId(command.UsuarioId);
                if (usuario == null || usuario.EmpresaId != command.EmpresaId)
                {
                    response.AddErro("Usuário não encontrado.");
                    return response;
                }

                // Evita duplicar contato com o mesmo telefone na mesma empresa (ja aconteceu
                // com dados de teste e quebrava o agrupamento de mensagens no chat, deixando
                // conversas do mesmo lead espalhadas em duas linhas diferentes).
                var existente = await _unitOfWork.Contato.ObterPorTelefone(command.EmpresaId, command.Telefone);
                if (existente != null)
                {
                    response.AddErro("Já existe um contato cadastrado com esse telefone.");
                    return response;
                }

                await _unitOfWork.Contato.Incluir(new Entidades.Contato(command));

                response.AddValue(new CriaContatoResult());
                _unitOfWork.Commit();
            }
            catch (Exception ex)
            {
                    _unitOfWork.Rollback();

                response.AddErroServico(ex, _logger, nameof(CriaContatoHandler));
            }

            return response;
        }
    }
}
