using Microsoft.Extensions.Logging;
using ProjetoMetaMensagem.Dominio.Common;
using ProjetoMetaMensagem.Dominio.Help.Error;
using ProjetoMetaMensagem.Dominio.Interfaces;
using ProjetoMetaMensagem.Dominio.Interfaces.Mediator;
using ProjetoMetaMensagem.Dominio.Interfaces.Servicos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjetoMetaMensagem.Dominio.UseCases.Empresa.AlteraEmpresa
{
    public class AlteraEmpresaHandler : IRequestHandler<AlteraEmpresaCommand, Response<AlteraEmpresaResult>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMetaService _metaService;

        private readonly ILogger<AlteraEmpresaHandler> _logger;

        public AlteraEmpresaHandler(IUnitOfWork unitOfWork, IMetaService metaService, ILogger<AlteraEmpresaHandler> logger)
        {
            _unitOfWork = unitOfWork;
            _metaService = metaService;
            _logger = logger;
        }

        public async Task<Response<AlteraEmpresaResult>> Handle(AlteraEmpresaCommand command)
        {
            var response = new Response<AlteraEmpresaResult>();

            try
            {
                _unitOfWork.BeginTransaction();
                // Validação: criar e usar um validador específico (AlteraEmpresaValidator) similar ao CriaEmpresaValidator
                var validator = new AlteraEmpresaValidator();
                var validateResult = validator.Validate(command);

                if (!validateResult.IsValid)
                {
                    response.AddErros(validateResult.Errors.ToCustomValidationFailure());
                    return response;
                }

                // Recupera entidade existente
                var existente = await _unitOfWork.Empresa.ObterPorId(command.Id);
                if (existente == null)
                {
                    response.AddErro("Empresa não encontrada.");
                    return response;
                }

                // Atualiza propriedades
                existente.Nome = command.Nome;
                existente.Email = command.Email;
                existente.Telefone = command.Telefone;
                existente.Cnpj = command.Cnpj;

                // A listagem devolve o token da Meta mascarado (ex: "••••bC7d"), entao a tela
                // reenvia a mascara quando o usuario nao troca o token. Gravar isso apagaria a
                // credencial da empresa e derrubaria todos os envios dela. So sobrescreve
                // quando chega um valor de verdade.
                var tokenNovo = command.AccessToken;
                var ehMascara = !string.IsNullOrWhiteSpace(tokenNovo)
                    && tokenNovo.StartsWith(ObtemEmpresa.ObtemEmpresaResult.PrefixoMascara, StringComparison.Ordinal);

                if (!string.IsNullOrWhiteSpace(tokenNovo) && !ehMascara)
                {
                    existente.MetaAccessToken = tokenNovo;
                }
                existente.WabaId = command.WabaId;
                existente.PhoneNumberId = command.PhoneNumberId;
                existente.AppIdMeta = command.AppIdMeta;
                existente.PlanoId = command.PlanoId;

                await _unitOfWork.Empresa.Alterar(existente);
                _unitOfWork.Commit();
            }
            catch (Exception ex)
            {
                    _unitOfWork.Rollback();
                
                response.AddErro(TratamentoErro.Tratar(ex, _logger, nameof(AlteraEmpresaHandler)));
            }

            return response;
        }
    }
}


