using Microsoft.Extensions.Logging;
using ProjetoMetaMensagem.Dominio.Common;
using ProjetoMetaMensagem.Dominio.Help.Error;
using ProjetoMetaMensagem.Dominio.Interfaces.Mediator;
using ProjetoMetaMensagem.Dominio.Interfaces.Repositorios;
using System;
using System.Threading.Tasks;

namespace ProjetoMetaMensagem.Dominio.UseCases.Template.ExcluiTemplateConexao
{
    public class ExcluiTemplateConexaoHandler : IRequestHandler<ExcluiTemplateConexaoCommand, Response<ExcluiTemplateConexaoResult>>
    {
        private readonly ITemplateConexaoRepository _templateConexaoRepository;

        private readonly ILogger<ExcluiTemplateConexaoHandler> _logger;

        public ExcluiTemplateConexaoHandler(ITemplateConexaoRepository templateConexaoRepository, ILogger<ExcluiTemplateConexaoHandler> logger)
        {
            _templateConexaoRepository = templateConexaoRepository;
            _logger = logger;
        }

        public async Task<Response<ExcluiTemplateConexaoResult>> Handle(ExcluiTemplateConexaoCommand command)
        {
            var response = new Response<ExcluiTemplateConexaoResult>();

            try
            {
                var validator = new ExcluiTemplateConexaoValidator();
                var validateResult = validator.Validate(command);

                if (!validateResult.IsValid)
                {
                    response.AddErros(validateResult.Errors.ToCustomValidationFailure());
                    return response;
                }

                var linhasAfetadas = await _templateConexaoRepository.Excluir(command.Id, command.EmpresaIdSolicitante);

                // Zero linhas significa que a conexao nao existe OU pertence a outra empresa.
                // As duas situacoes devolvem a mesma mensagem de proposito: dizer "existe, mas
                // nao e sua" ja entregaria ao atacante que aquele id e valido.
                if (linhasAfetadas == 0)
                {
                    response.AddErro("Conexão de template não encontrada.");
                    return response;
                }

                response.AddValue(new ExcluiTemplateConexaoResult());
            }
            catch (Exception ex)
            {
                response.AddErro(TratamentoErro.Tratar(ex, _logger, nameof(ExcluiTemplateConexaoHandler)));
            }

            return response;
        }
    }
}
