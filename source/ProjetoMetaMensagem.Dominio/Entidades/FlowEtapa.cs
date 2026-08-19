using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjetoMetaMensagem.Dominio.Entidades
{
    public class FlowEtapa
    {
        public FlowEtapa()
        {

        }

        public Guid Id { get; set; }
        public Guid FlowId { get; set; }
        public Guid? TemplateId { get; set; }
        public string NomeEtapa { get; set; }
        public string ConteudoLivre { get; set; }
        public string GatilhoResposta { get; set; }
        public Guid? ProximaEtapaId { get; set; }
        public bool EhEtapaInicial { get; set; }

        // Nome da variavel onde a resposta do cliente e guardada (etapas "Capturar Input").
        // Ex: "nome" -> a resposta vira {{nome}} nas mensagens seguintes do flow.
        public string? VariavelSaida { get; set; }

        // Quando preenchidos numa etapa "Capturar Input", o ConteudoLivre e enviado como
        // mensagem interativa de botoes (ate 20 caracteres cada) em vez de texto simples, e a
        // resposta do cliente (o titulo do botao tocado) e capturada normalmente em VariavelSaida.
        public string? Botao1 { get; set; }
        public string? Botao2 { get; set; }

        // Segundo caminho: quando a resposta do cliente casa com Botao2, o flow salta pra ca em
        // vez de seguir por ProximaEtapaId. E a unica ramificacao do motor -- so duas saidas,
        // porque a etapa tambem so oferece dois botoes. NULL = etapa segue linear como antes.
        public Guid? ProximaEtapaIdB { get; set; }
    }
}
