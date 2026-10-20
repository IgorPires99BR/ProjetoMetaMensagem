using Microsoft.Extensions.Logging;
using ProjetoMetaMensagem.Dominio.Common;
using ProjetoMetaMensagem.Dominio.Entidades;
using ProjetoMetaMensagem.Dominio.Help.Error;
using ProjetoMetaMensagem.Dominio.Interfaces;
using ProjetoMetaMensagem.Dominio.Interfaces.Mediator;
using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace ProjetoMetaMensagem.Dominio.UseCases.Template.SalvaGatilhosTemplate
{
    public class SalvaGatilhosTemplateHandler : IRequestHandler<SalvaGatilhosTemplateCommand, Response<SalvaGatilhosTemplateResult>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<SalvaGatilhosTemplateHandler> _logger;

        public SalvaGatilhosTemplateHandler(IUnitOfWork unitOfWork, ILogger<SalvaGatilhosTemplateHandler> logger)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async Task<Response<SalvaGatilhosTemplateResult>> Handle(SalvaGatilhosTemplateCommand command)
        {
            var response = new Response<SalvaGatilhosTemplateResult>();

            var validateResult = new SalvaGatilhosTemplateValidator().Validate(command);
            if (!validateResult.IsValid)
            {
                response.AddErros(validateResult.Errors.ToCustomValidationFailure());
                return response;
            }

            try
            {
                var template = await _unitOfWork.Template.ObterPorIdEEmpresa(command.TemplateId, command.EmpresaIdSolicitante);
                if (template == null)
                {
                    response.AddErro("Template não encontrado.");
                    return response;
                }

                // Sem um valor para cada {{n}} do corpo a Meta recusa todo envio automatico, e
                // ninguem esta olhando a tela na hora para perceber.
                var totalVariaveis = Regex.Matches(template.Conteudo ?? string.Empty, @"\{\{\d+\}\}").Count;
                if (command.Gatilhos.Any(g => (g.Variaveis?.Count ?? 0) != totalVariaveis))
                {
                    response.AddErro($"O modelo tem {totalVariaveis} campo(s) que mudam: defina de onde sai cada um em todos os momentos de disparo.");
                    return response;
                }

                var parametros = (await _unitOfWork.Parametro.ObterPorEmpresa(template.EmpresaId)).Select(p => p.Id).ToHashSet();
                if (command.Gatilhos.SelectMany(g => g.Variaveis ?? new()).Any(v => v.ParametroId.HasValue && !parametros.Contains(v.ParametroId.Value)))
                {
                    response.AddErro("Um dos parâmetros escolhidos não existe mais. Atualize a tela e selecione novamente.");
                    return response;
                }

                var gatilhos = command.Gatilhos.Select(g => new TemplateGatilho
                {
                    EmpresaId = template.EmpresaId,
                    TemplateId = template.Id,
                    Tipo = g.Tipo,
                    DiasAtraso = g.Tipo == TemplateGatilho.AtrasoPagamento ? g.DiasAtraso : null,
                    Ativo = g.Ativo,
                    Variaveis = g.Variaveis ?? new()
                }).ToList();

                _unitOfWork.BeginTransaction();
                await _unitOfWork.Template.SubstituirGatilhos(template.Id, template.EmpresaId, gatilhos);
                _unitOfWork.Commit();

                response.AddValue(new SalvaGatilhosTemplateResult { TemplateId = template.Id, TotalGatilhos = gatilhos.Count });
            }
            catch (Exception ex)
            {
                _unitOfWork.Rollback();
                response.AddErroServico(ex, _logger, nameof(SalvaGatilhosTemplateHandler));
            }

            return response;
        }
    }
}
