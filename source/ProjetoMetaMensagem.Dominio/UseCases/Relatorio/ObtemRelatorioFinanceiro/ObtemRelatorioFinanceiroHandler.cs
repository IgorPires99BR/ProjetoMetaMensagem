using ProjetoMetaMensagem.Dominio.Help.Error;
using Microsoft.Extensions.Logging;
using ProjetoMetaMensagem.Dominio.Common;
using ProjetoMetaMensagem.Dominio.Interfaces;
using ProjetoMetaMensagem.Dominio.Interfaces.Mediator;
using System;
using System.Threading.Tasks;

namespace ProjetoMetaMensagem.Dominio.UseCases.Relatorio.ObtemRelatorioFinanceiro
{
    public class ObtemRelatorioFinanceiroHandler : IRequestHandler<ObtemRelatorioFinanceiroCommand, Response<ObtemRelatorioFinanceiroResult>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<ObtemRelatorioFinanceiroHandler> _logger;

        public ObtemRelatorioFinanceiroHandler(IUnitOfWork unitOfWork, ILogger<ObtemRelatorioFinanceiroHandler> logger)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async Task<Response<ObtemRelatorioFinanceiroResult>> Handle(ObtemRelatorioFinanceiroCommand command)
        {
            var response = new Response<ObtemRelatorioFinanceiroResult>();

            var validator = new ObtemRelatorioFinanceiroValidator();
            var validateResult = validator.Validate(command);
            if (!validateResult.IsValid)
            {
                response.AddErros(validateResult.Errors.ToCustomValidationFailure());
                return response;
            }

            // Gasto por cliente e um dado financeiro da plataforma inteira -- so admin ve.
            if (!command.SolicitanteEhAdmin)
            {
                response.AddErro("Apenas administradores podem ver o relatório financeiro.");
                return response;
            }

            try
            {
                var gastos = await _unitOfWork.Relatorio.ListarGastoPorEmpresaMes(null, command.DataInicio, command.DataFim);
                response.AddValue(new ObtemRelatorioFinanceiroResult { Gastos = gastos });
            }
            catch (Exception ex)
            {
                response.AddErroServico(ex, _logger, nameof(ObtemRelatorioFinanceiroHandler));
            }

            return response;
        }
    }
}
