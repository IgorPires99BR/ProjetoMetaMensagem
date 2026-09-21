using Microsoft.Extensions.Logging;
using ProjetoMetaMensagem.Dominio.Common;
using ProjetoMetaMensagem.Dominio.Help.Error;
using ProjetoMetaMensagem.Dominio.Interfaces;
using ProjetoMetaMensagem.Dominio.Interfaces.Mediator;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace ProjetoMetaMensagem.Dominio.UseCases.Perfil.DeletaPerfil
{
    public class DeletaPerfilHandler : IRequestHandler<DeletaPerfilCommand, Response<DeletaPerfilResult>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<DeletaPerfilHandler> _logger;

        public DeletaPerfilHandler(IUnitOfWork unitOfWork, ILogger<DeletaPerfilHandler> logger)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async Task<Response<DeletaPerfilResult>> Handle(DeletaPerfilCommand command)
        {
            var response = new Response<DeletaPerfilResult>();

            try
            {
                _unitOfWork.BeginTransaction();
                var validator = new DeletaPerfilValidator();
                var validateResult = validator.Validate(command);

                if (!validateResult.IsValid)
                {
                    response.AddErros(validateResult.Errors.ToCustomValidationFailure());
                    return response;
                }

                var linhasAfetadas = await _unitOfWork.Perfil.Excluir(command.Id, command.EmpresaIdSolicitante);

                if (linhasAfetadas == 0)
                {
                    _unitOfWork.Rollback();
                    response.AddErro("Perfil não encontrado.");
                    return response;
                }

                response.AddValue(new DeletaPerfilResult());
                _unitOfWork.Commit();
            }
            catch (Exception ex)
            {
                _unitOfWork.Rollback();
                // FK_Usuario_Perfil bloqueia a exclusao quando algum usuario ainda usa este
                // perfil -- mensagem amigavel em vez do erro cru do SQL Server.
                if (ex.Message.Contains("FK_Usuario_Perfil", StringComparison.OrdinalIgnoreCase))
                {
                    response.AddErro("Este perfil está em uso por um ou mais usuários. Troque o perfil deles antes de excluir.");
                    return response;
                }

                response.AddErroServico(ex, _logger, nameof(DeletaPerfilHandler));
            }

            return response;
        }
    }
}
