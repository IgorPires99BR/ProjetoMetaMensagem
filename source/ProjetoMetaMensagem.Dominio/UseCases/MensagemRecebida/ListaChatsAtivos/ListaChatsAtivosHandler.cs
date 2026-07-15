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
                // 1. Busca os chats/conversas ativas daquela empresa específica
                // Se o seu repositório ainda não tiver esse método exato, você pode buscar pelas mensagens agrupadas por contato.
                var conversas = new List<Entidades.MensagemRecebida>();

                if (command.IdContato != null && command.IdContato.GetValueOrDefault() != Guid.Empty)
                {
                    conversas = await _unitOfWork.MensagemRecebida.ListarPorContato(command.IdEmpresa, command.IdContato.Value);
                }
                else
                {
                    conversas = await _unitOfWork.MensagemRecebida.ListarPorEmpresa(command.IdEmpresa);
                }


                var resultFinal = new ListaChatsAtivosResult();

                // Alimenta a lista do Result diretamente
                resultFinal.Chats = conversas
                 .GroupBy(m => m.ContatoId)
                 .Select(grupo =>
                 {
                     // Ordena para pegar a mensagem mais recente do grupo
                     var maisRecente = grupo.OrderByDescending(m => m.DataRecebimento).First();

                     // Conta quantas mensagens deste contato específico estão não lidas
                     var naoLidas = grupo.Count(m => !m.Lida && m.Tipo == "recebida");

                     return new ChatAtivoObjeto
                     {
                         // Como ContatoId é Guid?, usamos .GetValueOrDefault() por segurança
                         ContatoId = grupo.Key.GetValueOrDefault(),
                         NomeContato = "Contato " + maisRecente.TelefoneRemetente,
                         Telefone = maisRecente.TelefoneRemetente,
                         UltimaMensagem = maisRecente.Conteudo, // 'c.u' corrigido para o campo correto da entidade
                         DataUltimaMensagem = maisRecente.DataRecebimento,
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
