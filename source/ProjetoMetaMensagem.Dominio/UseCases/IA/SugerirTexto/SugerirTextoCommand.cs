using ProjetoMetaMensagem.Dominio.Common;
using ProjetoMetaMensagem.Dominio.Interfaces.Mediator;

namespace ProjetoMetaMensagem.Dominio.UseCases.IA.SugerirTexto
{
    public class SugerirTextoCommand : IRequest<Response<SugerirTextoResult>>
    {
        // O que pedir pra IA (ex: "Sugira uma resposta para a mensagem do cliente").
        // Cada tela do painel monta a sua propria, o backend so sabe conversar com o Gemini.
        public string Instrucao { get; set; } = string.Empty;

        // Texto de referencia opcional (historico da conversa, conteudo atual do campo etc).
        public string? Contexto { get; set; }

        // Quantas alternativas gerar. 1 tambem serve pra pedir uma unica resposta/explicacao.
        public int Quantidade { get; set; } = 3;
    }
}
