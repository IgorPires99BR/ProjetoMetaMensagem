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
    }
}
