using System;

namespace ProjetoMetaMensagem.Dominio.UseCases.Template.ListaTemplateConexoes
{
    public class ListaTemplateConexoesResult
    {
        public ListaTemplateConexoesResult(Entidades.TemplateConexao conexao)
        {
            Id = conexao.Id;
            EmpresaId = conexao.EmpresaId;
            TemplateOrigemId = conexao.TemplateOrigemId;
            TemplateDestinoId = conexao.TemplateDestinoId;
            BotaoTexto = conexao.BotaoTexto;
            DataCriacao = conexao.DataCriacao;
        }

        public Guid Id { get; set; }
        public Guid EmpresaId { get; set; }
        public Guid TemplateOrigemId { get; set; }
        public Guid TemplateDestinoId { get; set; }
        public string? BotaoTexto { get; set; }
        public DateTime DataCriacao { get; set; }
    }
}
