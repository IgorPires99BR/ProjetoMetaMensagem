using ProjetoMetaMensagem.Dominio.UseCases.Flows.CriaFlow;
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

        public Flow(CriaFlowCommand command) : this()
        {
            EmpresaId = command.IdEmpresa;
            Nome = command.Nome;
            Descricao = command.Descricao;
            Ativo = true;
            GatilhoInicial = command.GatilhoPalavraChave; // Campo ideal para salvar o start do bot
        }

        public Guid Id { get; set; }
        public Guid EmpresaId { get; set; }
        public string Nome { get; set; }
        public string Descricao { get; set; }
        public string GatilhoInicial { get; set; } // Adicionado para persistir o gatilho principal
        public bool Ativo { get; set; }
        public DateTime DataCriacao { get; set; }

        // Propriedade de navegação para as etapas
        public List<FlowEtapa> Etapas { get; set; }
    }
}
