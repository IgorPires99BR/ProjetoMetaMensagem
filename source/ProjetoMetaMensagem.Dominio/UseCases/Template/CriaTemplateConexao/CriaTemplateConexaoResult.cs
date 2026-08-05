using System;

namespace ProjetoMetaMensagem.Dominio.UseCases.Template.CriaTemplateConexao
{
    public class CriaTemplateConexaoResult
    {
        public Guid Id { get; set; }
        public Guid EmpresaId { get; set; }
        public Guid TemplateOrigemId { get; set; }
        public Guid TemplateDestinoId { get; set; }
        public string? BotaoTexto { get; set; }
        public DateTime DataCriacao { get; set; }
    }
}
