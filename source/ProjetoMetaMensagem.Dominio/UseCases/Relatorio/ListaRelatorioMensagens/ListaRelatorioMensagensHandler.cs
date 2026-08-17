using ProjetoMetaMensagem.Dominio.Help.Error;
using Microsoft.Extensions.Logging;
using ProjetoMetaMensagem.Dominio.Common;
using ProjetoMetaMensagem.Dominio.Helpers.MensagemFormatter;
using ProjetoMetaMensagem.Dominio.Interfaces;
using ProjetoMetaMensagem.Dominio.Interfaces.Mediator;
using System;
using System.Threading.Tasks;

namespace ProjetoMetaMensagem.Dominio.UseCases.Relatorio.ListaRelatorioMensagens
{
    public class ListaRelatorioMensagensHandler : IRequestHandler<ListaRelatorioMensagensCommand, Response<ListaRelatorioMensagensResult>>
    {
        private readonly IUnitOfWork _unitOfWork;

        private readonly ILogger<ListaRelatorioMensagensHandler> _logger;

        public ListaRelatorioMensagensHandler(IUnitOfWork unitOfWork, ILogger<ListaRelatorioMensagensHandler> logger)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async Task<Response<ListaRelatorioMensagensResult>> Handle(ListaRelatorioMensagensCommand command)
        {
            var response = new Response<ListaRelatorioMensagensResult>();

            var validator = new ListaRelatorioMensagensValidator();
            var validateResult = validator.Validate(command);
            if (!validateResult.IsValid)
            {
                response.AddErros(validateResult.Errors.ToCustomValidationFailure());
                return response;
            }

            try
            {
                var mensagens = await _unitOfWork.Relatorio.ListarMensagens(
                    command.EmpresaId, command.DataInicio, command.DataFim, command.Pagina, command.TamanhoPagina);

                // Mesmo tratamento do chat: o historico antigo guarda o payload do template em
                // JSON, e sem formatar o relatorio exibia {"ParametrosBody":...} na coluna de
                // conteudo, onde deveria estar a mensagem.
                foreach (var mensagem in mensagens)
                {
                    mensagem.Conteudo = MensagemFormatter.FormatarConteudo(mensagem.Conteudo);
                }

                response.AddValue(new ListaRelatorioMensagensResult { Mensagens = mensagens });
            }
            catch (Exception ex)
            {
                response.AddErroServico(ex, _logger, nameof(ListaRelatorioMensagensHandler));
            }

            return response;
        }
    }
}
