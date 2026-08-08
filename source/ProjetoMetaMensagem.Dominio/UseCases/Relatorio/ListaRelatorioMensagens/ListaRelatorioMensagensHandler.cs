using ProjetoMetaMensagem.Dominio.Help.Error;
using Microsoft.Extensions.Logging;
using ProjetoMetaMensagem.Dominio.Common;
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
