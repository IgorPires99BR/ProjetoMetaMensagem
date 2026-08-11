using ProjetoMetaMensagem.Dominio.Entidades;
using ProjetoMetaMensagem.Dominio.UseCases.Flows.ListaFlows;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Intrinsics.X86;
using System.Text;
using System.Threading.Tasks;

namespace ProjetoMetaMensagem.Dominio.UseCases.Flows.ListaFlows
{
    public class ListaFlowsResult
    {
        public ListaFlowsResult(Flow flow)
        {
            Id = flow.Id;
            IdEmpresa = flow.EmpresaId;
            Nome = flow.Nome;
            Descricao = flow.Descricao;
            GatilhoPalavraChave = flow.GatilhoInicial;
            Ativo = flow.Ativo;
            NumeroId = flow.NumeroId;

            // Mapeia a lista de etapas internas caso elas tenham sido carregadas pelo repositório
            if (flow.Etapas != null)
            {
                Etapas = flow.Etapas
                    .Select(etapa => new ListaFlowsEtapaDto
                    {
                        Id = etapa.Id,
                        NomeEtapa = etapa.NomeEtapa,
                        ConteudoLivre = etapa.ConteudoLivre,
                        GatilhoResposta = etapa.GatilhoResposta,
                        ProximaEtapaId = etapa.ProximaEtapaId,
                        EhEtapaInicial = etapa.EhEtapaInicial,
                        TemplateId = etapa.TemplateId
                    })
                    .ToList();
            }
        }

        public Guid Id { get; set; }
        public Guid IdEmpresa { get; set; }
        public string Nome { get; set; }
        public string Descricao { get; set; }
        public string GatilhoPalavraChave { get; set; }
        public bool Ativo { get; set; }
        public Guid? NumeroId { get; set; }

        // Mapeia diretamente o array para o front-end ler como 'etapas'
        public List<ListaFlowsEtapaDto> Etapas { get; set; } = new List<ListaFlowsEtapaDto>();
    }

    public class ListaFlowsEtapaDto
    {
        public Guid Id { get; set; }
        public string NomeEtapa { get; set; }
        public string ConteudoLivre { get; set; }
        public string GatilhoResposta { get; set; }
        public Guid? ProximaEtapaId { get; set; }
        public bool EhEtapaInicial { get; set; }
        public Guid? TemplateId { get; set; }
    }
}
