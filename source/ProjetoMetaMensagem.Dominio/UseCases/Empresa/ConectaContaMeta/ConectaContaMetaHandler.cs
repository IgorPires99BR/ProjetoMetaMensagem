using Microsoft.Extensions.Logging;
using ProjetoMetaMensagem.Dominio.Common;
using ProjetoMetaMensagem.Dominio.Help.Error;
using ProjetoMetaMensagem.Dominio.Interfaces;
using ProjetoMetaMensagem.Dominio.Interfaces.Mediator;
using ProjetoMetaMensagem.Dominio.Interfaces.Servicos;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace ProjetoMetaMensagem.Dominio.UseCases.Empresa.ConectaContaMeta
{
    public class ConectaContaMetaHandler : IRequestHandler<ConectaContaMetaCommand, Response<ConectaContaMetaResult>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMetaService _metaService;
        private readonly ILogger<ConectaContaMetaHandler> _logger;

        public ConectaContaMetaHandler(IUnitOfWork unitOfWork, IMetaService metaService, ILogger<ConectaContaMetaHandler> logger)
        {
            _unitOfWork = unitOfWork;
            _metaService = metaService;
            _logger = logger;
        }

        public async Task<Response<ConectaContaMetaResult>> Handle(ConectaContaMetaCommand command)
        {
            var response = new Response<ConectaContaMetaResult>();

            var validator = new ConectaContaMetaValidator();
            var validateResult = validator.Validate(command);
            if (!validateResult.IsValid)
            {
                response.AddErros(validateResult.Errors.ToCustomValidationFailure());
                return response;
            }

            try
            {
                _unitOfWork.BeginTransaction();

                var empresa = await _unitOfWork.Empresa.ObterPorId(command.EmpresaId);
                if (empresa == null)
                {
                    _unitOfWork.Rollback();
                    response.AddErro("Empresa não encontrada.");
                    return response;
                }

                // Troca o "code" do Embedded Signup pelo token de sistema (short-lived) --
                // mesma chamada usada pelo Embedded Signup a nivel de Numero.
                var shortLivedToken = await _metaService.TrocarCodeEmbeddedSignupAsync(command.Code);

                if (string.IsNullOrEmpty(shortLivedToken))
                {
                    _unitOfWork.Rollback();
                    response.AddErro("A Meta aceitou a requisição, mas não retornou um token de sistema válido.");
                    return response;
                }

                // Sem essa troca o token expira rapido (short-lived). Falha aqui nao deve
                // impedir a conexao: cai pro token curto mesmo, fica pendente de renovacao
                // (mesmo comportamento do IniciaEmbeddedSignupHandler).
                var accessToken = shortLivedToken;
                try
                {
                    var longLived = await _metaService.TrocarTokenLongLivedAsync(shortLivedToken);
                    accessToken = longLived.Token;
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Falha ao trocar token do Embedded Signup por long-lived, mantendo o token curto. Empresa {EmpresaId}", command.EmpresaId);
                }

                await _unitOfWork.Empresa.AtualizarCredenciaisMeta(
                    command.EmpresaId, command.WabaId, command.PhoneNumberId, accessToken);

                _unitOfWork.Commit();

                // Assina o app no WABA depois do commit: efeito colateral na Meta, nao no nosso
                // banco -- uma falha aqui nao deve derrubar a conexao que ja foi persistida.
                var appAssinado = !string.IsNullOrEmpty(command.WabaId)
                    && await _metaService.AssinarAppNoWabaAsync(command.WabaId, accessToken);

                if (!appAssinado)
                {
                    _logger.LogWarning("Falha ao assinar o app no WABA {WabaId} apos conectar a Empresa {EmpresaId}. A empresa ficara sem receber mensagens ate uma sincronizacao bem sucedida.", command.WabaId, command.EmpresaId);
                }

                response.AddValue(new ConectaContaMetaResult
                {
                    Sucesso = true,
                    WabaId = command.WabaId,
                    PhoneNumberId = command.PhoneNumberId,
                    AppAssinado = appAssinado
                });
            }
            catch (Exception ex)
            {
                _unitOfWork.Rollback();
                response.AddErroServico(ex, _logger, nameof(ConectaContaMetaHandler));
            }

            return response;
        }
    }
}
