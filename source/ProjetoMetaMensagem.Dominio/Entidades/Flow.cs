using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjetoMetaMensagem.Dominio.Entidades
{
    public class Flow
    {
        public Flow()
        {
            Id = Guid.NewGuid();
            DataCriacao = DateTime.Now;
            Etapas = new List<FlowEtapa>();
        }

        public Guid Id { get; set; }
        public Guid EmpresaId { get; set; }
        public string Nome { get; set; }
        public string Descricao { get; set; }
        public bool Ativo { get; set; }
        public DateTime DataCriacao { get; set; }

        public virtual Empresa Empresa { get; set; }
        public virtual ICollection<FlowEtapa> Etapas { get; set; }
    }
}
