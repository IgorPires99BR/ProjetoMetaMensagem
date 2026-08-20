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

                // Troca o "code" do Embedded Signup pelo token de sistema (short-lived)
                var shortLivedToken = await _metaService.TrocarCodeEmbeddedSignupAsync(command.Code);

                if (string.IsNullOrEmpty(shortLivedToken))
                {
                    response.AddErro("A Meta aceitou a requisição, mas não retornou um token de sistema válido.");
                    _unitOfWork.Rollback();
                    return response;
                }

                // Sem essa troca o token expira rapido (short-lived) e o numero "desconecta"
                // sozinho pouco tempo depois do onboarding. Falha aqui nao deve travar o
                // cadastro: cai pro token curto mesmo, e fica pendente de renovacao.
                var systemUserToken = shortLivedToken;
                DateTime? tokenExpiraEm = null;
                try
                {
                    var longLived = await _metaService.TrocarTokenLongLivedAsync(shortLivedToken);
                    systemUserToken = longLived.Token;
                    tokenExpiraEm = longLived.ExpiraEm;
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Falha ao trocar token do Embedded Signup por long-lived, mantendo o token curto. Empresa {IdEmpresa}", command.IdEmpresa);
                }

                var wabaId = command.WabaId ?? await _unitOfWork.Empresa.ObterWabaId(command.IdEmpresa);

                var novoNumero = new Entidades.Numero
                {
                    Id = Guid.NewGuid(),
                    UsuarioId = command.UsuarioId,
                    Telefone = command.NumeroTelefone,
                    Descricao = command.NomeEmpresa,
                    // InstanciaId = phone_number_id da Meta. Sem gravar aqui, o numero fica sem
                    // identificador valido e AtivaCoexistenciaHandler falha logo na primeira
                    // tentativa ("O número ainda não possui um identificador válido na Meta").
                    InstanciaId = command.PhoneNumberId,
                    WabaId = wabaId,
                    SystemUserToken = systemUserToken,
                    TokenExpiraEm = tokenExpiraEm,
                    TipoConexao = TipoConexaoNumero.ApiOficial,
                    StatusConexao = "Conectado",
                    StatusMeta = "PENDING",
                    QualidadeMeta = "UNKNOWN",
                    DataCriacao = DateTime.Now
                };

                await _unitOfWork.Numero.Incluir(novoNumero);
                _unitOfWork.Commit();

                // Assina o app nos webhooks do WABA depois do commit: e um efeito colateral na
                // Meta, nao no nosso banco, entao uma falha aqui nao deve derrubar o cadastro
                // que ja foi persistido -- so fica pendente de retry via "Sincronizar Meta"
                // (AtualizaNumeroMetaHandler tambem chama isso a cada sincronizacao).
                var appAssinado = !string.IsNullOrEmpty(wabaId) && await _metaService.AssinarAppNoWabaAsync(wabaId, systemUserToken);
                if (!appAssinado)
                {
                    _logger.LogWarning("Falha ao assinar o app no WABA {WabaId} apos Embedded Signup. Numero {NumeroId} ficara sem receber mensagens ate uma sincronizacao bem sucedida.", wabaId, novoNumero.Id);
                }

                response.AddValue(new IniciaEmbeddedSignupResult
                {
                    NumeroId = novoNumero.Id,
                    StatusConexao = novoNumero.StatusConexao,
                    AppAssinado = appAssinado
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
