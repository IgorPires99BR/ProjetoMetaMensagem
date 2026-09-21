using Microsoft.Extensions.Logging;
using ProjetoMetaMensagem.Dominio.Common;
using ProjetoMetaMensagem.Dominio.Help.Error;
using ProjetoMetaMensagem.Dominio.Interfaces;
using ProjetoMetaMensagem.Dominio.Interfaces.Mediator;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace ProjetoMetaMensagem.Dominio.UseCases.Perfil.AlteraPerfil
{
    public class AlteraPerfilHandler : IRequestHandler<AlteraPerfilCommand, Response<AlteraPerfilResult>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<AlteraPerfilHandler> _logger;

        public AlteraPerfilHandler(IUnitOfWork unitOfWork, ILogger<AlteraPerfilHandler> logger)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async Task<Response<AlteraPerfilResult>> Handle(AlteraPerfilCommand command)
        {
            var response = new Response<AlteraPerfilResult>();

            try
            {
                _unitOfWork.BeginTransaction();
                var validator = new AlteraPerfilValidator();
                var validateResult = validator.Validate(command);

                if (!validateResult.IsValid)
                {
                    response.AddErros(validateResult.Errors.ToCustomValidationFailure());
                    return response;
                }

                var perfil = new Entidades.Perfil { Id = command.Id, Nome = command.Nome, Telas = command.Telas };
                var linhasAfetadas = await _unitOfWork.Perfil.Alterar(perfil, command.EmpresaIdSolicitante);

                if (linhasAfetadas == 0)
                {
                    _unitOfWork.Rollback();
                    response.AddErro("Perfil não encontrado.");
                    return response;
                }

                response.AddValue(new AlteraPerfilResult());
                _unitOfWork.Commit();
            }
            catch (Exception ex)
            {
                _unitOfWork.Rollback();
                response.AddErroServico(ex, _logger, nameof(AlteraPerfilHandler));
            }

            return response;
        }
    }
}
