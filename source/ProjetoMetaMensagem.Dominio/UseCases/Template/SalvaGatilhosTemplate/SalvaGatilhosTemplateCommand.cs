using ProjetoMetaMensagem.Dominio.Common;
using ProjetoMetaMensagem.Dominio.Entidades;
using ProjetoMetaMensagem.Dominio.Interfaces.Mediator;
using System;
using System.Collections.Generic;

namespace ProjetoMetaMensagem.Dominio.UseCases.Template.SalvaGatilhosTemplate
{
    // Troca de uma vez todos os momentos de disparo do template pelos informados. Lista vazia
    // desliga todos. Configuracao local (a Meta nao sabe dela), entao vale ate em template aprovado.
    public class SalvaGatilhosTemplateCommand : IRequest<Response<SalvaGatilhosTemplateResult>>
    {
        public Guid TemplateId { get; set; }

        // Escopo sempre vindo do token -- mesmo padrao de AlteraFlagCobrancaTemplateCommand.
        public Guid? EmpresaIdSolicitante { get; set; }

        public List<GatilhoTemplateDto> Gatilhos { get; set; } = new();
    }

    public class GatilhoTemplateDto
    {
        public string Tipo { get; set; }
        public int? DiasAtraso { get; set; }
        public bool Ativo { get; set; } = true;
        public List<AgendamentoVariavelDto> Variaveis { get; set; } = new();
    }
}
