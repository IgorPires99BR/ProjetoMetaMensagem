using ProjetoMetaMensagem.Dominio.Help.Error;
using Microsoft.Extensions.Logging;
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

        private readonly ILogger<ListaChatsAtivosHandler> _logger;

        public ListaChatsAtivosHandler(IUnitOfWork unitOfWork, ILogger<ListaChatsAtivosHandler> logger)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async Task<Response<ListaChatsAtivosResult>> Handle(ListaChatsAtivosCommand command)
        {
            var response = new Response<ListaChatsAtivosResult>();

            try
            {
                var validator = new ListaChatsAtivosValidator();
                var validateResult = validator.Validate(command);

                if (!validateResult.IsValid)
                {
                    response.AddErros(validateResult.Errors.ToCustomValidationFailure());
                    return response;
                }

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

                // Sem esse JOIN o nome do contato nunca aparecia no chat -- a tela sempre
                // mostrava "Contato <telefone>", mesmo pra quem tinha nome cadastrado, e a
                // busca por nome na lista de conversas ficava inutil. Busca em lote (nao um
                // ObterPorId por grupo) pra nao virar N+1 na tela mais acessada do sistema.
                var idsContatos = itensRecebidos.Select(i => i.ContatoId)
                    .Concat(itensEnviados.Select(i => i.ContatoId))
                    .Where(id => id != Guid.Empty)
                    .Distinct()
                    .ToList();

                var contatos = await _unitOfWork.Contato.ObterPorIds(command.IdEmpresa, idsContatos);

                // GroupBy + First em vez de ToDictionary direto: esta e a tela mais acessada do
                // sistema e ela NAO pode cair por causa do formato do dado. ToDictionary estoura
                // com chave repetida, e foi exatamente assim que a tela quebrou (500) quando o
                // banco tinha conversa duplicada pro mesmo contato. O indice unico (BD/34) evita
                // a duplicata na origem; isto aqui garante que, mesmo se algo escapar, a tela
                // continua carregando em vez de derrubar o atendimento inteiro.
                var nomePorContato = contatos
                    .Where(c => !string.IsNullOrWhiteSpace(c.Nome))
                    .GroupBy(c => c.Id)
                    .ToDictionary(g => g.Key, g => g.First().Nome!);

                // Estado de flow ativo por contato (bot vs vendedor), buscado em lote pelo mesmo
                // motivo do ObterPorIds acima: evitar N+1 na tela mais acessada do sistema.
                var estadosAtivos = await _unitOfWork.ConversationState.ObterAtivasPorEmpresaEContatos(command.IdEmpresa, idsContatos);
                var estadoPorContato = estadosAtivos
                    .GroupBy(e => e.ContatoId)
                    .ToDictionary(g => g.Key, g => g.OrderByDescending(e => e.DataAtualizacao).First());

                var resultFinal = new ListaChatsAtivosResult();

                // Alimenta a lista do Result diretamente
                resultFinal.Chats = itensRecebidos
                 .Concat(itensEnviados)
                 // Quem ainda nao virou contato entra com ContatoId vazio; agrupar so por ele
                 // jogava TODOS esses numeros diferentes numa unica linha de conversa. Sem
                 // contato cadastrado, o telefone e que identifica a conversa.
                 .GroupBy(m => m.ContatoId != Guid.Empty ? m.ContatoId.ToString() : "tel:" + m.Telefone)
                 .Select(grupo =>
                 {
                     // Ordena para pegar a mensagem mais recente do grupo
                     var maisRecente = grupo.OrderByDescending(m => m.Data).First();

                     // Conta quantas mensagens deste contato específico estão não lidas
                     var naoLidas = grupo.Count(m => m.NaoLida);

                     var contatoId = maisRecente.ContatoId;
                     estadoPorContato.TryGetValue(contatoId, out var estado);

                     return new ChatAtivoObjeto
                     {
                         ContatoId = contatoId,
                         NomeContato = nomePorContato.TryGetValue(contatoId, out var nome) ? nome : "Contato " + maisRecente.Telefone,
                         Telefone = maisRecente.Telefone,
                         UltimaMensagem = maisRecente.Conteudo,
                         DataUltimaMensagem = maisRecente.Data,
                         QuantidadeNaoLidas = naoLidas,
                         // Aguardando atendente NAO conta como "respondendo por flow": o bot
                         // parou de conduzir, entao a tela nao pode sugerir que esta coberto.
                         RespondendoPorFlow = estado != null && estado.AssumidoPorUsuarioId == null && !estado.AguardandoAtendente,
                         FlowAssumido = estado != null && estado.AssumidoPorUsuarioId != null,
                         AguardandoAtendente = estado != null && estado.AguardandoAtendente && estado.AssumidoPorUsuarioId == null
                     };
                 })
                 // Sem isso a lista saia na ordem "de agrupamento" (basicamente aleatoria),
                 // entao quem mandou a mensagem mais recente nao subia pro topo da lista.
                 .OrderByDescending(c => c.DataUltimaMensagem)
                 .ToList();

                response.AddValue(resultFinal);
            }
            catch (Exception ex)
            {
                response.AddErroServico(ex, _logger, nameof(ListaChatsAtivosHandler));
            }

            return response;
        }
    }
}
