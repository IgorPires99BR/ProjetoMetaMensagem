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
            Id = Guid.NewGuid();
        }

        public Guid Id { get; set; }
        public Guid FlowId { get; set; }
        public Guid? TemplateId { get; set; }
        public string NomeEtapa { get; set; }
        public string ConteudoLivre { get; set; }
        public string GatilhoResposta { get; set; }
        public Guid? ProximaEtapaId { get; set; }
        public bool EhEtapaInicial { get; set; }
    }
}
