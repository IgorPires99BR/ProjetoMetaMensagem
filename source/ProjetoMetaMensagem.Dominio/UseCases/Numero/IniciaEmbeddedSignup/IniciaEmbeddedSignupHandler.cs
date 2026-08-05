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

        public IniciaEmbeddedSignupHandler(IUnitOfWork unitOfWork, IMetaService metaService)
        {
            _unitOfWork = unitOfWork;
            _metaService = metaService;
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
                response.AddErro($"Falha ao concluir o Embedded Signup: {ex.Message}");
            }

            return response;
        }
    }
}
