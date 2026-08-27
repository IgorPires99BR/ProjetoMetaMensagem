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
            NumeroId = command.NumeroId;
            SourceIdAnuncio = command.SourceIdAnuncio;
        }

        public Guid Id { get; set; }
        public Guid EmpresaId { get; set; }
        public string Nome { get; set; }
        public string Descricao { get; set; }
        public string GatilhoInicial { get; set; } // Adicionado para persistir o gatilho principal
        public bool Ativo { get; set; }
        public DateTime DataCriacao { get; set; }
        // NULL = flow vale para todos os numeros da empresa. Preenchido = flow exclusivo
        // daquele numero (tem prioridade sobre um flow generico com o mesmo gatilho).
        public Guid? NumeroId { get; set; }

        // Id do anuncio (source_id do referral) que leva a este flow. Quem chega por esse
        // anuncio cai aqui direto, sem depender do texto que digitou -- o gatilho por texto
        // quebra quando a pessoa apaga a mensagem sugerida e escreve outra coisa.
        // NULL = flow escolhido pelo gatilho de texto, como sempre.
        public string? SourceIdAnuncio { get; set; }

        // Propriedade de navegação para as etapas
        public List<FlowEtapa> Etapas { get; set; }
    }
}
