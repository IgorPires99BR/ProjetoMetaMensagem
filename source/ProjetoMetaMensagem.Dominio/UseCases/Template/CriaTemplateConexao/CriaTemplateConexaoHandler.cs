using Microsoft.Extensions.Logging;
using ProjetoMetaMensagem.Dominio.Common;
using ProjetoMetaMensagem.Dominio.Help.Error;
using ProjetoMetaMensagem.Dominio.Interfaces.Mediator;
using ProjetoMetaMensagem.Dominio.Interfaces.Repositorios;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace ProjetoMetaMensagem.Dominio.UseCases.Template.CriaTemplateConexao
{
    public class CriaTemplateConexaoHandler : IRequestHandler<CriaTemplateConexaoCommand, Response<CriaTemplateConexaoResult>>
    {
        private readonly ITemplateConexaoRepository _templateConexaoRepository;

        private readonly ILogger<CriaTemplateConexaoHandler> _logger;

        public CriaTemplateConexaoHandler(ITemplateConexaoRepository templateConexaoRepository, ILogger<CriaTemplateConexaoHandler> logger)
        {
            _templateConexaoRepository = templateConexaoRepository;
            _logger = logger;
        }

        public async Task<Response<CriaTemplateConexaoResult>> Handle(CriaTemplateConexaoCommand command)
        {
            var response = new Response<CriaTemplateConexaoResult>();

            try
            {
                var validator = new CriaTemplateConexaoValidator();
                var validateResult = validator.Validate(command);

                if (!validateResult.IsValid)
                {
                    response.AddErros(validateResult.Errors.ToCustomValidationFailure());
                    return response;
                }

                var conexao = new Entidades.TemplateConexao
                {
                    Id = Guid.NewGuid(),
                    EmpresaId = command.EmpresaId,
                    TemplateOrigemId = command.TemplateOrigemId,
                    TemplateDestinoId = command.TemplateDestinoId,
                    BotaoTexto = command.BotaoTexto,
                    DataCriacao = DateTime.Now
                };

                await _templateConexaoRepository.Incluir(conexao);

                response.AddValue(new CriaTemplateConexaoResult
                {
                    Id = conexao.Id,
                    EmpresaId = conexao.EmpresaId,
                    TemplateOrigemId = conexao.TemplateOrigemId,
                    TemplateDestinoId = conexao.TemplateDestinoId,
                    BotaoTexto = conexao.BotaoTexto,
                    DataCriacao = conexao.DataCriacao
                });
            }
            catch (Exception ex)
            {
                response.AddErroServico(ex, _logger, nameof(CriaTemplateConexaoHandler));
            }

            return response;
        }
    }
}
