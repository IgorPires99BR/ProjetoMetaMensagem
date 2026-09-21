using Microsoft.Extensions.Logging;
using ProjetoMetaMensagem.Dominio.Common;
using ProjetoMetaMensagem.Dominio.Help.Error;
using ProjetoMetaMensagem.Dominio.Interfaces;
using ProjetoMetaMensagem.Dominio.Interfaces.Mediator;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace ProjetoMetaMensagem.Dominio.UseCases.Perfil.CriaPerfil
{
    public class CriaPerfilHandler : IRequestHandler<CriaPerfilCommand, Response<CriaPerfilResult>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<CriaPerfilHandler> _logger;

        public CriaPerfilHandler(IUnitOfWork unitOfWork, ILogger<CriaPerfilHandler> logger)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async Task<Response<CriaPerfilResult>> Handle(CriaPerfilCommand command)
        {
            var response = new Response<CriaPerfilResult>();

            try
            {
                if (command.EmpresaIdSolicitante.HasValue)
                {
                    command.EmpresaId = command.EmpresaIdSolicitante.Value;
                }

                _unitOfWork.BeginTransaction();
                var validator = new CriaPerfilValidator();
                var validateResult = validator.Validate(command);

                if (!validateResult.IsValid)
                {
                    response.AddErros(validateResult.Errors.ToCustomValidationFailure());
                    return response;
                }

                var perfil = new Entidades.Perfil
                {
                    EmpresaId = command.EmpresaId,
                    Nome = command.Nome,
                    Telas = command.Telas
                };

                await _unitOfWork.Perfil.Incluir(perfil);

                response.AddValue(new CriaPerfilResult { Id = perfil.Id });
                _unitOfWork.Commit();
            }
            catch (Exception ex)
            {
                _unitOfWork.Rollback();
                response.AddErroServico(ex, _logger, nameof(CriaPerfilHandler));
            }

            return response;
        }
    }
}
