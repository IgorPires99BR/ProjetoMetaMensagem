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
            var mensagensRecebidasTask = await _unitOfWork.MensagemRecebida
                 .ListarPorContato(command.EmpresaId, command.ContatoId);

            var historicoEnviadasTask = await _unitOfWork.HistoricoDisparo
                .ListarPorContato(command.EmpresaId, command.ContatoId);

            // 2. Mapeia as recebidas (cliente -> user)
            var listaRecebidas = mensagensRecebidasTask.Select(m => new
            {
                m.Id,
                From = "user",
                Text = m.Conteudo,
                Data = m.DataRecebimento
            });

            // 3. Mapeia as enviadas (sistema/empresa -> bot ou me)
            var listaEnviadas = historicoEnviadasTask.Select(h => new
            {
                h.Id,
                From = "bot", // Altere para "me" ou "bot" conforme o padrão da sua UI no Angular
                Text = h.Conteudo,
                Data = h.DataEnvio
            });

            // 4. Une as duas listas e ordena pela data/hora real do evento
            var resultadoDto = listaRecebidas
                .Concat(listaEnviadas)
                .OrderBy(x => x.Data)
                .Select(x => new ItemMensagemChatDto
                {
                    Id = x.Id,
                    From = x.From,
                    Text = x.Text,
                    Time = x.Data.ToString("HH:mm")
                })
                .ToList();

            var resultFinal = new ListaMensagemRecebidaResult { Mensagens = resultadoDto };
            response.AddValue(resultFinal);

            return response;
        }
    }
}
