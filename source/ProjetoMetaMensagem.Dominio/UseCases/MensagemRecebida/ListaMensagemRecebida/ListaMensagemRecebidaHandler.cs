using ProjetoMetaMensagem.Dominio.Common;
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

        public ListaMensagemRecebidaHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async  Task<Response<ListaMensagemRecebidaResult>> Handle(ListaMensagemRecebidaCommand command)
        {
            var response = new Response<ListaMensagemRecebidaResult>();

            // Busca as mensagens trocadas com este contato específico nesta empresa
            var mensagens = await _unitOfWork.MensagemRecebida
                .ListarPorContato(command.EmpresaId, command.ContatoId);

            // Mapeia para um DTO que o Angular entenda (id, texto, quem enviou, hora)
            var resultadoDto = mensagens.Select(m => new ItemMensagemChatDto
            {
                Id = m.Id,
                From = m.Tipo == "recebida" ? "user" : "bot",
                Text = m.Conteudo,
                Time = m.DataRecebimento.ToString("HH:mm")
            }).ToList();

            var resultFinal = new ListaMensagemRecebidaResult { Mensagens = resultadoDto };

            response.AddValue(resultFinal);

            return response;
        }
    }
}
