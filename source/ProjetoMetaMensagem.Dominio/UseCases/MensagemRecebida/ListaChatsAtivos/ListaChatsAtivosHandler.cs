using ProjetoMetaMensagem.Dominio.Common;
using ProjetoMetaMensagem.Dominio.Interfaces;
using ProjetoMetaMensagem.Dominio.Interfaces.Mediator;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjetoMetaMensagem.Dominio.UseCases.MensagemRecebida.ListaChatsAtivos
{
    public class ListaChatsAtivosHandler : IRequestHandler<ListaChatsAtivosCommand, Response<ListaChatsAtivosResult>>
    {
        private readonly IUnitOfWork _unitOfWork;

        public ListaChatsAtivosHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<Response<ListaChatsAtivosResult>> Handle(ListaChatsAtivosCommand command)
        {
            var response = new Response<ListaChatsAtivosResult>();

            try
            {
                // 1. Busca os chats/conversas ativas daquela empresa específica, combinando mensagens
                // recebidas e disparos enviados — sem isso, contatos que só receberam um disparo
                // (e ainda não responderam) não apareciam na lista de conversas.
                var conversas = new List<Entidades.MensagemRecebida>();
                var disparos = new List<Entidades.HistoricoDisparoComTelefone>();

                if (command.IdContato != null && command.IdContato.GetValueOrDefault() != Guid.Empty)
                {
                    conversas = await _unitOfWork.MensagemRecebida.ListarPorContato(command.IdEmpresa, command.IdContato.Value);
                    var disparosContato = await _unitOfWork.HistoricoDisparo.ListarPorContato(command.IdEmpresa, command.IdContato.Value);
                    disparos = disparosContato.Select(h => new Entidades.HistoricoDisparoComTelefone
                    {
                        Id = h.Id,
                        EmpresaId = h.EmpresaId,
                        ContatoId = h.ContatoId,
                        DataEnvio = h.DataEnvio
                    }).ToList();
                }
                else
                {
                    conversas = await _unitOfWork.MensagemRecebida.ListarPorEmpresa(command.IdEmpresa);
                    disparos = (await _unitOfWork.HistoricoDisparo.ListarPorEmpresa(command.IdEmpresa)).ToList();
                }

                // Unifica os dois lados em um formato comum (ContatoId, Telefone, Conteudo, Data, Lida)
                var itensRecebidos = conversas.Select(m => new
                {
                    ContatoId = m.ContatoId.GetValueOrDefault(),
                    Telefone = m.TelefoneRemetente,
                    Conteudo = m.Conteudo,
                    Data = m.DataRecebimento,
                    NaoLida = !m.Lida && m.Tipo == "recebida"
                });

                var itensEnviados = disparos.Select(h => new
                {
                    ContatoId = h.ContatoId,
                    Telefone = h.TelefoneContato,
                    Conteudo = "📄 Mensagem enviada",
                    Data = h.DataEnvio,
                    NaoLida = false
                });

                var resultFinal = new ListaChatsAtivosResult();

                // Alimenta a lista do Result diretamente
                resultFinal.Chats = itensRecebidos
                 .Concat(itensEnviados)
                 .GroupBy(m => m.ContatoId)
                 .Select(grupo =>
                 {
                     // Ordena para pegar a mensagem mais recente do grupo
                     var maisRecente = grupo.OrderByDescending(m => m.Data).First();

                     // Conta quantas mensagens deste contato específico estão não lidas
                     var naoLidas = grupo.Count(m => m.NaoLida);

                     return new ChatAtivoObjeto
                     {
                         ContatoId = grupo.Key,
                         NomeContato = "Contato " + maisRecente.Telefone,
                         Telefone = maisRecente.Telefone,
                         UltimaMensagem = maisRecente.Conteudo,
                         DataUltimaMensagem = maisRecente.Data,
                         QuantidadeNaoLidas = naoLidas
                     };
                 })
                 .ToList();

                response.AddValue(resultFinal);
            }
            catch (Exception ex)
            {
                response.AddErro($"Erro ao listar chats ativos: {ex.Message}");
            }

            return response;
        }
    }
}
