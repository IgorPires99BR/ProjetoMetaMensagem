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

        public AlteraEmpresaHandler(IUnitOfWork unitOfWork, IMetaService metaService)
        {
            _unitOfWork = unitOfWork;
            _metaService = metaService;
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
                existente.MetaAccessToken = command.AccessToken;
                existente.PlanoId = command.PlanoId;

                await _unitOfWork.Empresa.Alterar(existente);
                _unitOfWork.Commit();
            }
            catch (Exception ex)
            {
                    _unitOfWork.Rollback();
                
                response.AddErro($"Erro: {ex.Message}");
            }

            return response;
        }
    }
}


