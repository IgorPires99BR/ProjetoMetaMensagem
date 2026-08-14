using ProjetoMetaMensagem.Dominio.Help.Error;
using Microsoft.Extensions.Logging;
using ProjetoMetaMensagem.Dominio.Common;
using ProjetoMetaMensagem.Dominio.Interfaces;
using ProjetoMetaMensagem.Dominio.Interfaces.Mediator;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace ProjetoMetaMensagem.Dominio.UseCases.Relatorio.ObtemRelatorioEngajamento
{
    public class ObtemRelatorioEngajamentoHandler : IRequestHandler<ObtemRelatorioEngajamentoCommand, Response<ObtemRelatorioEngajamentoResult>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<ObtemRelatorioEngajamentoHandler> _logger;

        public ObtemRelatorioEngajamentoHandler(IUnitOfWork unitOfWork, ILogger<ObtemRelatorioEngajamentoHandler> logger)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async Task<Response<ObtemRelatorioEngajamentoResult>> Handle(ObtemRelatorioEngajamentoCommand command)
        {
            var response = new Response<ObtemRelatorioEngajamentoResult>();

            var validator = new ObtemRelatorioEngajamentoValidator();
            var validateResult = validator.Validate(command);
            if (!validateResult.IsValid)
            {
                response.AddErros(validateResult.Errors.ToCustomValidationFailure());
                return response;
            }

            // Quem nao e admin so enxerga a propria empresa, igual ObtemEmpresaHandler.
            Guid? empresaAlvo;
            if (!command.SolicitanteEhAdmin)
            {
                if (command.EmpresaIdSolicitante == null)
                {
                    response.AddErro("Não foi possível identificar a empresa do usuário logado.");
                    return response;
                }
                empresaAlvo = command.EmpresaIdSolicitante.Value;
            }
            else
            {
                empresaAlvo = command.EmpresaIdFiltro;
            }

            try
            {
                var engajamento = await _unitOfWork.Relatorio.ListarEngajamento(empresaAlvo, command.DataInicio, command.DataFim);

                var resultado = new ObtemRelatorioEngajamentoResult
                {
                    Empresas = engajamento.Select(e => new EngajamentoEmpresaResultDto
                    {
                        EmpresaId = e.EmpresaId,
                        NomeEmpresa = e.NomeEmpresa,
                        Enviados = e.Enviados,
                        Visualizaram = e.Visualizaram,
                        Responderam = e.Responderam,
                        NaoResponderam = e.Enviados - e.Responderam
                    }).ToList()
                };

                response.AddValue(resultado);
            }
            catch (Exception ex)
            {
                response.AddErroServico(ex, _logger, nameof(ObtemRelatorioEngajamentoHandler));
            }

            return response;
        }
    }
}
