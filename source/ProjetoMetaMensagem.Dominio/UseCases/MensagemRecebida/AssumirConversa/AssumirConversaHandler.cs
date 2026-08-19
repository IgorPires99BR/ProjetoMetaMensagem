using ProjetoMetaMensagem.Dominio.Help.Error;
using Microsoft.Extensions.Logging;
using ProjetoMetaMensagem.Dominio.Common;
using ProjetoMetaMensagem.Dominio.Interfaces;
using ProjetoMetaMensagem.Dominio.Interfaces.Mediator;
using System;
using System.Threading.Tasks;

namespace ProjetoMetaMensagem.Dominio.UseCases.MensagemRecebida.AssumirConversa
{
    // Pausa o robô numa conversa sem precisar mandar mensagem. Antes o único jeito de assumir
    // era responder, o que obriga o atendente a escrever algo antes de conseguir ler a conversa
    // com calma -- e enquanto isso o bot seguia respondendo por cima dele.
    public class AssumirConversaHandler : IRequestHandler<AssumirConversaCommand, Response<AssumirConversaResult>>
    {
        private readonly IUnitOfWork _unitOfWork;

        private readonly ILogger<AssumirConversaHandler> _logger;

        public AssumirConversaHandler(IUnitOfWork unitOfWork, ILogger<AssumirConversaHandler> logger)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async Task<Response<AssumirConversaResult>> Handle(AssumirConversaCommand command)
        {
            var response = new Response<AssumirConversaResult>();

            var validator = new AssumirConversaValidator();
            var validateResult = validator.Validate(command);
            if (!validateResult.IsValid)
            {
                response.AddErros(validateResult.Errors.ToCustomValidationFailure());
                return response;
            }

            try
            {
                var estadoAtivo = await _unitOfWork.ConversationState.ObterPorEmpresaEContato(command.EmpresaId, command.ContatoId);

                // Sem conversa ativa nao ha nada pra pausar: o contato pode ter terminado o flow
                // ou nunca ter entrado em um. Nao e erro -- a tela so nao marca como assumida.
                if (estadoAtivo == null)
                {
                    response.AddValue(new AssumirConversaResult { Assumida = false });
                    return response;
                }

                if (estadoAtivo.AssumidoPorUsuarioId == null)
                {
                    await _unitOfWork.ConversationState.AssumirManualmente(estadoAtivo.Id, command.UsuarioId);
                }

                response.AddValue(new AssumirConversaResult { Assumida = true });
            }
            catch (Exception ex)
            {
                response.AddErroServico(ex, _logger, nameof(AssumirConversaHandler));
            }

            return response;
        }
    }
}
