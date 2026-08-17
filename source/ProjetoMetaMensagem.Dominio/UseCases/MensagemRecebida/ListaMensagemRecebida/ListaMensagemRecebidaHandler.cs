using Microsoft.Extensions.Logging;
using ProjetoMetaMensagem.Dominio.Common;
using ProjetoMetaMensagem.Dominio.Help.Error;
using ProjetoMetaMensagem.Dominio.Helpers.MensagemFormatter;
using ProjetoMetaMensagem.Dominio.Interfaces;
using ProjetoMetaMensagem.Dominio.Interfaces.Mediator;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjetoMetaMensagem.Dominio.UseCases.MensagemRecebida.ListaMensagemRecebida
{
    public class ListaMensagemRecebidaHandler : IRequestHandler<ListaMensagemRecebidaCommand, Response<ListaMensagemRecebidaResult>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<ListaMensagemRecebidaHandler> _logger;

        public ListaMensagemRecebidaHandler(IUnitOfWork unitOfWork, ILogger<ListaMensagemRecebidaHandler> logger)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async Task<Response<ListaMensagemRecebidaResult>> Handle(ListaMensagemRecebidaCommand command)
        {
            var response = new Response<ListaMensagemRecebidaResult>();

            try
            {
                var validator = new ListaMensagemRecebidaValidator();
                var validateResult = validator.Validate(command);

                if (!validateResult.IsValid)
                {
                    response.AddErros(validateResult.Errors.ToCustomValidationFailure());
                    return response;
                }

                // Busca ja unificada e paginada no banco (MensagemRecebida + HistoricoDisparo
                // numa unica ordenacao por data) -- paginar cada tabela separado e juntar depois
                // (como era antes) desalinhava a conversa: um flow manda varias mensagens pra
                // cada resposta do cliente, entao os dois cursores de paginacao andavam em
                // ritmos diferentes e a conversa aparecia fora de ordem ao rolar pra cima.
                var itens = await _unitOfWork.MensagemRecebida
                    .ListarConversaUnificadaPaginada(command.EmpresaId, command.ContatoId, command.Pagina, command.TamanhoPagina);

                // A busca vem desc (mais recente primeiro, pra paginacao); devolve em ordem
                // cronologica (asc) pro chat renderizar de cima pra baixo.
                var resultadoDto = itens
                    .OrderBy(x => x.Data)
                    .Select(x => new ItemMensagemChatDto
                    {
                        Id = x.Id,
                        From = x.Origem,
                        Text = x.Origem == "bot" ? MensagemFormatter.FormatarConteudo(x.Texto) : (x.Texto ?? string.Empty),
                        Time = x.Data.ToString("HH:mm"),
                        Wamid = x.Wamid,
                        Status = x.Status,
                        Erro = x.Erro,
                        MidiaId = x.MidiaId,
                        TipoMidia = x.TipoMidia
                    })
                    .ToList();

                var resultFinal = new ListaMensagemRecebidaResult { Mensagens = resultadoDto };
                response.AddValue(resultFinal);
            }
            catch (Exception ex)
            {
                response.AddErroServico(ex, _logger, nameof(ListaMensagemRecebidaHandler));
            }

            return response;
        }
    }
}
