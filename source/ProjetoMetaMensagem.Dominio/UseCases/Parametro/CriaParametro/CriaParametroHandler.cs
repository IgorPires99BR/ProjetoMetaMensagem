using Microsoft.Extensions.Logging;
using ProjetoMetaMensagem.Dominio.Common;
using ProjetoMetaMensagem.Dominio.Help.Error;
using ProjetoMetaMensagem.Dominio.Interfaces;
using ProjetoMetaMensagem.Dominio.Interfaces.Mediator;
using System;
using System.Threading.Tasks;

namespace ProjetoMetaMensagem.Dominio.UseCases.Parametro.CriaParametro
{
    public class CriaParametroHandler : IRequestHandler<CriaParametroCommand, Response<CriaParametroResult>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<CriaParametroHandler> _logger;

        public CriaParametroHandler(IUnitOfWork unitOfWork, ILogger<CriaParametroHandler> logger)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async Task<Response<CriaParametroResult>> Handle(CriaParametroCommand command)
        {
            var response = new Response<CriaParametroResult>();

            try
            {
                var validateResult = new CriaParametroValidator().Validate(command);
                if (!validateResult.IsValid)
                {
                    response.AddErros(validateResult.Errors.ToCustomValidationFailure());
                    return response;
                }

                _unitOfWork.BeginTransaction();

                var parametro = new Entidades.Parametro
                {
                    EmpresaId = command.EmpresaId,
                    Nome = command.Nome.Trim(),
                    Descricao = command.Descricao?.Trim(),
                    Tipo = command.Tipo,
                    Valor = command.Valor.Trim()
                };

                await _unitOfWork.Parametro.Incluir(parametro);

                response.AddValue(new CriaParametroResult { Id = parametro.Id });
                _unitOfWork.Commit();
            }
            catch (Exception ex)
            {
                _unitOfWork.Rollback();
                // UQ_Parametro_Empresa_Nome: mensagem amigavel em vez do erro cru do SQL Server.
                if (ex.Message.Contains("UQ_Parametro_Empresa_Nome", StringComparison.OrdinalIgnoreCase))
                {
                    response.AddErro("Já existe um parâmetro com esse nome nesta empresa.");
                    return response;
                }

                response.AddErroServico(ex, _logger, nameof(CriaParametroHandler));
            }

            return response;
        }
    }
}
