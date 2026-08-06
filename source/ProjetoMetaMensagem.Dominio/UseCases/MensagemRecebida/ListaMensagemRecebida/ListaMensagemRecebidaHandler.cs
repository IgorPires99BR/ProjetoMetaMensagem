using ProjetoMetaMensagem.Dominio.Common;
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

        public ListaMensagemRecebidaHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<Response<ListaMensagemRecebidaResult>> Handle(ListaMensagemRecebidaCommand command)
        {
            var response = new Response<ListaMensagemRecebidaResult>();

            // Busca ate TamanhoPagina de cada fonte na mesma pagina; como sao duas tabelas
            // diferentes intercaladas por data, o merge abaixo corta o total certo, mas o
            // "corte" de cada fonte individualmente e uma aproximacao (assume distribuicao
            // razoavelmente equilibrada entre enviadas/recebidas por pagina).
            var mensagensRecebidasTask = await _unitOfWork.MensagemRecebida
                 .ListarPorContatoPaginado(command.EmpresaId, command.ContatoId, command.Pagina, command.TamanhoPagina);

            var historicoEnviadasTask = await _unitOfWork.HistoricoDisparo
                .ListarPorContatoPaginado(command.EmpresaId, command.ContatoId, command.Pagina, command.TamanhoPagina);

            // 2. Mapeia as recebidas (cliente -> user). Sem wamid/status: confirmação de
            // entrega/leitura só existe pro lado que nós enviamos.
            var listaRecebidas = mensagensRecebidasTask.Select(m => new
            {
                m.Id,
                From = "user",
                Text = m.Conteudo,
                Data = m.DataRecebimento,
                Wamid = (string?)null,
                Status = (string?)null
            });

            // 3. Mapeia as enviadas tratando o JSON de disparo de template.
            // From = "bot" (mesmo valor usado pelo Angular pro lado direito do chat).
            var listaEnviadas = historicoEnviadasTask.Select(h => new
            {
                h.Id,
                From = "bot",
                Text = MensagemFormatter.FormatarConteudo(h.Conteudo),
                Data = h.DataEnvio,
                Wamid = (string?)h.WamidMeta,
                Status = h.StatusEntrega
            });

            // 4. Une as duas listas, pega as TamanhoPagina mais recentes (desc) e devolve em
            // ordem cronologica (asc) pro chat renderizar de cima pra baixo
            var resultadoDto = listaRecebidas
                .Concat(listaEnviadas)
                .OrderByDescending(x => x.Data)
                .Take(command.TamanhoPagina)
                .OrderBy(x => x.Data)
                .Select(x => new ItemMensagemChatDto
                {
                    Id = x.Id,
                    From = x.From,
                    Text = x.Text,
                    Time = x.Data.ToString("HH:mm"),
                    Wamid = x.Wamid,
                    Status = x.Status
                })
                .ToList();

            var resultFinal = new ListaMensagemRecebidaResult { Mensagens = resultadoDto };
            response.AddValue(resultFinal);

            return response;
        }
    }
}
