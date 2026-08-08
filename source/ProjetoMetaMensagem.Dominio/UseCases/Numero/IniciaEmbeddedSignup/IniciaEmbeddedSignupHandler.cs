using Microsoft.Extensions.Logging;
using ProjetoMetaMensagem.Dominio.Common;
using ProjetoMetaMensagem.Dominio.Enums;
using ProjetoMetaMensagem.Dominio.Help.Error;
using ProjetoMetaMensagem.Dominio.Interfaces;
using ProjetoMetaMensagem.Dominio.Interfaces.Mediator;
using ProjetoMetaMensagem.Dominio.Interfaces.Servicos;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace ProjetoMetaMensagem.Dominio.UseCases.Numero.IniciaEmbeddedSignup
{
    public class IniciaEmbeddedSignupHandler : IRequestHandler<IniciaEmbeddedSignupCommand, Response<IniciaEmbeddedSignupResult>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMetaService _metaService;

        private readonly ILogger<IniciaEmbeddedSignupHandler> _logger;

        public IniciaEmbeddedSignupHandler(IUnitOfWork unitOfWork, IMetaService metaService, ILogger<IniciaEmbeddedSignupHandler> logger)
        {
            _unitOfWork = unitOfWork;
            _metaService = metaService;
            _logger = logger;
        }

        public async Task<Response<IniciaEmbeddedSignupResult>> Handle(IniciaEmbeddedSignupCommand command)
        {
            var response = new Response<IniciaEmbeddedSignupResult>();

            var validator = new IniciaEmbeddedSignupValidator();
            var validateResult = validator.Validate(command);

            if (!validateResult.IsValid)
            {
                response.AddErros(validateResult.Errors.ToCustomValidationFailure());
                return response;
            }

            try
            {
                _unitOfWork.BeginTransaction();

                // Numero nao tem EmpresaId proprio: pertence a empresa do UsuarioId gravado nele.
                // O EmpresaAccessFilter ja garante que IdEmpresa bate com o token de quem chama
                // (ou libera admin), mas nao tem como saber que UsuarioId aqui precisa ser da
                // MESMA empresa -- sem essa checagem, o numero cai na empresa dona do UsuarioId
                // informado, nao necessariamente a IdEmpresa declarada.
                var usuarioDoNumero = await _unitOfWork.Usuario.ObterPorId(command.UsuarioId);
                if (usuarioDoNumero == null || usuarioDoNumero.EmpresaId != command.IdEmpresa)
                {
                    _unitOfWork.Rollback();
                    response.AddErro("Usuário não encontrado.");
                    return response;
                }

                // Troca o "code" do Embedded Signup pelo token de sistema
                var trocaCode = await _metaService.TrocarCodeEmbeddedSignupAsync(command.Code);

                if (trocaCode == null || string.IsNullOrEmpty(trocaCode.SystemUserToken))
                {
                    response.AddErro("A Meta aceitou a requisição, mas não retornou um token de sistema válido.");
                    _unitOfWork.Rollback();
                    return response;
                }

                var wabaId = await _unitOfWork.Empresa.ObterWabaId(command.IdEmpresa);

                var novoNumero = new Entidades.Numero
                {
                    Id = Guid.NewGuid(),
                    UsuarioId = command.UsuarioId,
                    Telefone = command.NumeroTelefone,
                    Descricao = command.NomeEmpresa,
                    WabaId = wabaId,
                    SystemUserToken = trocaCode.SystemUserToken,
                    TipoConexao = TipoConexaoNumero.ApiOficial,
                    StatusConexao = "Conectado",
                    StatusMeta = "PENDING",
                    QualidadeMeta = "UNKNOWN",
                    DataCriacao = DateTime.Now
                };

                await _unitOfWork.Numero.Incluir(novoNumero);
                _unitOfWork.Commit();

                response.AddValue(new IniciaEmbeddedSignupResult
                {
                    NumeroId = novoNumero.Id,
                    StatusConexao = novoNumero.StatusConexao
                });
            }
            catch (Exception ex)
            {
                _unitOfWork.Rollback();
                response.AddErroServico(ex, _logger, nameof(IniciaEmbeddedSignupHandler));
            }

            return response;
        }
    }
}
