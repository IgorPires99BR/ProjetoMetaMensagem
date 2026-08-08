using Microsoft.Extensions.Logging;
using ProjetoMetaMensagem.Dominio.Common;
using ProjetoMetaMensagem.Dominio.Help.Error;
using ProjetoMetaMensagem.Dominio.Interfaces;
using ProjetoMetaMensagem.Dominio.Interfaces.Mediator;
using ProjetoMetaMensagem.Dominio.Interfaces.Servicos;

namespace ProjetoMetaMensagem.Dominio.UseCases.Numero.ListarNumeros
{
    public class ListarNumerosHandler : IRequestHandler<ListarNumerosCommand, Response<List<ListarNumerosResult>>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMetaService _metaService;

        private readonly ILogger<ListarNumerosHandler> _logger;

        public ListarNumerosHandler(IUnitOfWork unitOfWork, IMetaService metaService, ILogger<ListarNumerosHandler> logger)
        {
            _unitOfWork = unitOfWork;
            _metaService = metaService;
            _logger = logger;
        }


        public async Task<Response<List<ListarNumerosResult>>> Handle(ListarNumerosCommand command)
        {
            var response = new Response<List<ListarNumerosResult>>();

            try
            {
                var listaNumeros = new List<ListarNumerosResult>();

                var validator = new ListarNumerosValidator();
                var validateResult = validator.Validate(command);

                if (!validateResult.IsValid)
                {
                    response.AddErros(validateResult.Errors.ToCustomValidationFailure());
                    return response;
                }

                var numerosBanco = await _unitOfWork.Numero.ObterPorUsuario(command.IdUsuario);

                if (numerosBanco == null)
                {
                    response.AddErro("Não foram encontrados números no banco de dados");
                    return response;
                }

                foreach (var numero in numerosBanco)
                {
                    listaNumeros.Add(new ListarNumerosResult(numero));
                }

                response.AddValue(listaNumeros);
            }
            catch (Exception ex)
            {
                response.AddErro(TratamentoErro.Tratar(ex, _logger, nameof(ListarNumerosHandler)));
            }

            return response;
        }
    }
}
