using ProjetoMetaMensagem.Dominio.Entidades;
using System;
using System.Collections.Generic;

namespace ProjetoMetaMensagem.Dominio.UseCases.Template.ListaGatilhosTemplate
{
    public class ListaGatilhosTemplateResult
    {
        public ListaGatilhosTemplateResult(TemplateGatilho gatilho)
        {
            Id = gatilho.Id;
            TemplateId = gatilho.TemplateId;
            Tipo = gatilho.Tipo;
            DiasAtraso = gatilho.DiasAtraso;
            Ativo = gatilho.Ativo;
            Variaveis = gatilho.Variaveis;
        }

        public Guid Id { get; set; }
        public Guid TemplateId { get; set; }
        public string Tipo { get; set; }
        public int? DiasAtraso { get; set; }
        public bool Ativo { get; set; }
        public List<AgendamentoVariavelDto> Variaveis { get; set; }
    }
}
