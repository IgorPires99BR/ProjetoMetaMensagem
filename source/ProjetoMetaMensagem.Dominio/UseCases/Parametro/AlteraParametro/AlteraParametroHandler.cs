using Microsoft.Extensions.Logging;
using ProjetoMetaMensagem.Dominio.Common;
using ProjetoMetaMensagem.Dominio.Help.Error;
using ProjetoMetaMensagem.Dominio.Interfaces;
using ProjetoMetaMensagem.Dominio.Interfaces.Mediator;
using System;
using System.Threading.Tasks;

namespace ProjetoMetaMensagem.Dominio.UseCases.Parametro.AlteraParametro
{
    public class AlteraParametroHandler : IRequestHandler<AlteraParametroCommand, Response<AlteraParametroResult>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<AlteraParametroHandler> _logger;

        public AlteraParametroHandler(IUnitOfWork unitOfWork, ILogger<AlteraParametroHandler> logger)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async Task<Response<AlteraParametroResult>> Handle(AlteraParametroCommand command)
        {
            var response = new Response<AlteraParametroResult>();

            try
            {
                var validateResult = new AlteraParametroValidator().Validate(command);
                if (!validateResult.IsValid)
                {
                    response.AddErros(validateResult.Errors.ToCustomValidationFailure());
                    return response;
                }

                _unitOfWork.BeginTransaction();

                var parametro = new Entidades.Parametro
                {
                    Id = command.Id,
                    Nome = command.Nome.Trim(),
                    Descricao = command.Descricao?.Trim(),
                    Tipo = command.Tipo,
                    Valor = command.Valor.Trim()
                };

                var linhasAfetadas = await _unitOfWork.Parametro.Alterar(parametro, command.EmpresaIdSolicitante);

                if (linhasAfetadas == 0)
                {
                    _unitOfWork.Rollback();
                    response.AddErro("Parâmetro não encontrado.");
                    return response;
                }

                response.AddValue(new AlteraParametroResult());
                _unitOfWork.Commit();
            }
            catch (Exception ex)
            {
                _unitOfWork.Rollback();
                if (ex.Message.Contains("UQ_Parametro_Empresa_Nome", StringComparison.OrdinalIgnoreCase))
                {
                    response.AddErro("Já existe um parâmetro com esse nome nesta empresa.");
                    return response;
                }

                response.AddErroServico(ex, _logger, nameof(AlteraParametroHandler));
            }

            return response;
        }
    }
}
