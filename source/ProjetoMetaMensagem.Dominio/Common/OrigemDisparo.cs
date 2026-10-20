using System;
using System.Collections.Generic;

namespace ProjetoMetaMensagem.Dominio.Common
{
    // De onde saiu um disparo, gravado em HistoricoDisparo.Origem e exibido no relatorio de
    // mensagens. So essas 4 origens existem hoje: Campanha (CampanhaWorker) conta como
    // DisparadorDeMensagem e o onboarding comercial automatico (OnboardingComercialService)
    // conta como FlowAutomatico -- nenhum dos dois e disparo manual nem agendamento do
    // cliente, mas se encaixam melhor nessas categorias do que criar uma 5a.
    public static class OrigemDisparo
    {
        public const string DisparadorDeMensagem = "Disparador de Mensagem";
        public const string AgendadorAutomatico = "Agendador Automático";
        public const string FlowAutomatico = "Flow Automático";
        public const string ChatManual = "Chat Manual";
        // Momento de disparo do template (ver TemplateGatilho). Fora de Validas de proposito:
        // so o proprio DisparoPorGatilho grava essa origem, nunca o corpo de uma requisicao.
        public const string GatilhoTemplate = "Gatilho de Template";

        private static readonly HashSet<string> Validas = new(StringComparer.OrdinalIgnoreCase)
        {
            DisparadorDeMensagem, AgendadorAutomatico, FlowAutomatico, ChatManual
        };

        // Usado pelas rotas compartilhadas entre a tela Disparador e o Chat manual, onde a
        // origem vem do corpo da requisicao (nao dá pra fixar no controller): cai pro
        // Disparador se vier vazio/invalido, pra nunca gravar um valor fora dos 4 esperados.
        public static string ResolverOuPadrao(string? origem) =>
            !string.IsNullOrWhiteSpace(origem) && Validas.Contains(origem) ? origem : DisparadorDeMensagem;
    }
}
