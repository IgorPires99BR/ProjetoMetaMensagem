using ProjetoMetaMensagem.Dominio.Common;
using ProjetoMetaMensagem.Dominio.Interfaces.Mediator;
using ProjetoMetaMensagem.Dominio.UseCases.Template.Common;
using ProjetoMetaMensagem.Dominio.UseCases.Template.CriaTemplate;
using System;
using System.Collections.Generic;

namespace ProjetoMetaMensagem.Dominio.UseCases.Template.AtualizaTemplate
{
    // Edita um template já criado (PENDING ou REJECTED), reenviando pra Meta via POST
    // /{template-id}. Propositalmente sem NomeTemplate/Idioma: a Meta não permite alterar nem um
    // nem outro numa edição — outro idioma ou nome exige criar um novo template.
    public class AtualizaTemplateCommand : IRequest<Response<AtualizaTemplateResult>>, ITemplateComponentesInput
    {
        public Guid TemplateId { get; set; }

        // Escopo sempre vindo do token (nunca do corpo/rota) -- mesmo padrao de DeletaTemplateCommand
        public Guid? EmpresaIdSolicitante { get; set; }

        public string Categoria { get; set; }
        public string Conteudo { get; set; }
        public List<CriaTemplateBotaoCommand>? Botoes { get; set; }

        public string? HeaderTipo { get; set; }
        public string? HeaderTexto { get; set; }
        public string? HeaderExemploHandle { get; set; }
        public string? FooterTexto { get; set; }
        public List<string>? ExemplosBody { get; set; }
    }
}
