using System;
using System.Collections.Generic;

namespace ProjetoMetaMensagem.Dominio.UseCases.Relatorio.ObtemFunilFlow
{
    public class ObtemFunilFlowResult
    {
        public Guid FlowId { get; set; }
        public string NomeFlow { get; set; } = string.Empty;

        public int TotalConversas { get; set; }

        // Concluiu o flow inteiro (chegou na ultima etapa e nao tinha pra onde ir).
        public int TotalConcluiram { get; set; }

        // Respondeu fora do esperado duas vezes seguidas e foi entregue a um atendente --
        // nao e abandono nem conclusao, e um terceiro desfecho que a tela precisa distinguir.
        public int TotalEntreguesAoAtendente { get; set; }

        // Ainda no meio da conversa, nem concluiu nem foi pro atendente -- pode responder a
        // qualquer momento, ou pode ser quem sumiu de vez. So o tempo dira qual.
        public int TotalPresas { get; set; }

        public List<FunilEtapaDto> Etapas { get; set; } = new();
    }

    public class FunilEtapaDto
    {
        public Guid EtapaId { get; set; }

        // Posicao no flow, calculada andando pelo encadeamento de etapas a partir da etapa
        // inicial -- o banco nao guarda uma ordem explicita (mesma logica que a tela de Flows
        // usa pra desenhar o fluxograma).
        public int Ordem { get; set; }

        public string NomeEtapa { get; set; } = string.Empty;

        // Texto da etapa, truncado -- e o que a pessoa realmente leu, mais util pra reconhecer
        // "qual pergunta" do que o tipo generico da etapa ("Capturar Input").
        public string? Rotulo { get; set; }

        public bool EhEtapaFinal { get; set; }

        public int Presas { get; set; }
        public int EntreguesAoAtendente { get; set; }
        public int Concluiram { get; set; }
    }
}
