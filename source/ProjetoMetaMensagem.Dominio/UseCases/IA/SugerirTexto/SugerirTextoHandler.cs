using ProjetoMetaMensagem.Dominio.Help.Error;
using Microsoft.Extensions.Logging;
using ProjetoMetaMensagem.Dominio.Common;
using ProjetoMetaMensagem.Dominio.Interfaces.Mediator;
using ProjetoMetaMensagem.Dominio.Interfaces.Servicos;

namespace ProjetoMetaMensagem.Dominio.UseCases.IA.SugerirTexto
{
    public class SugerirTextoHandler : IRequestHandler<SugerirTextoCommand, Response<SugerirTextoResult>>
    {
        private readonly IGeminiService _geminiService;
        private readonly ILogger<SugerirTextoHandler> _logger;

        public SugerirTextoHandler(IGeminiService geminiService, ILogger<SugerirTextoHandler> logger)
        {
            _geminiService = geminiService;
            _logger = logger;
        }

        public async Task<Response<SugerirTextoResult>> Handle(SugerirTextoCommand command)
        {
            var response = new Response<SugerirTextoResult>();

            var validator = new SugerirTextoValidator();
            var validateResult = validator.Validate(command);
            if (!validateResult.IsValid)
            {
                response.AddErros(validateResult.Errors.ToCustomValidationFailure());
                return response;
            }

            try
            {
                var opcoes = await _geminiService.SugerirAlternativas(command.Instrucao, command.Contexto, command.Quantidade);

                response.AddValue(new SugerirTextoResult { Opcoes = opcoes });
            }
            catch (Exception ex)
            {
                response.AddErroServico(ex, _logger, nameof(SugerirTextoHandler));
            }

            return response;
        }
    }
}
