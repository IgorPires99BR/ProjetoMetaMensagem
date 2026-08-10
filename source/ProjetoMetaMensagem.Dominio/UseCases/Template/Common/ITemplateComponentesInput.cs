using ProjetoMetaMensagem.Dominio.UseCases.Template.CriaTemplate;
using System.Collections.Generic;

namespace ProjetoMetaMensagem.Dominio.UseCases.Template.Common
{
    // Contrato comum aos campos de componentes (Header/Body/Footer/Botoes) compartilhado entre
    // criação (CriaTemplateCommand) e edição (AtualizaTemplateCommand) de template, para que os
    // dois handlers montem os componentes a partir de um único builder (TemplateComponentesBuilder).
    public interface ITemplateComponentesInput
    {
        string? HeaderTipo { get; }
        string? HeaderTexto { get; }
        string? HeaderExemploHandle { get; }
        string Conteudo { get; }
        List<string>? ExemplosBody { get; }
        string? FooterTexto { get; }
        List<CriaTemplateBotaoCommand>? Botoes { get; }
    }
}
