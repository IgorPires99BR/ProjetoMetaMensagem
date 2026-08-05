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

        public ExcluiTemplateConexaoHandler(ITemplateConexaoRepository templateConexaoRepository)
        {
            _templateConexaoRepository = templateConexaoRepository;
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

                await _templateConexaoRepository.Excluir(command.Id);

                response.AddValue(new ExcluiTemplateConexaoResult());
            }
            catch (Exception ex)
            {
                response.AddErro($"Erro: {ex.Message}");
            }

            return response;
        }
    }
}
