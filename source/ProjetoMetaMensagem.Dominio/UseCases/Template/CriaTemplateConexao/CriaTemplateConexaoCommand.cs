using ProjetoMetaMensagem.Dominio.Common;
using ProjetoMetaMensagem.Dominio.Interfaces.Mediator;
using System;

namespace ProjetoMetaMensagem.Dominio.UseCases.Template.CriaTemplateConexao
{
    public class CriaTemplateConexaoCommand : IRequest<Response<CriaTemplateConexaoResult>>
    {
        public Guid EmpresaId { get; set; }
        public Guid TemplateOrigemId { get; set; }
        public Guid TemplateDestinoId { get; set; }
        public string? BotaoTexto { get; set; }
    }
}
