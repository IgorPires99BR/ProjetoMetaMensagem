using System;

namespace ProjetoMetaMensagem.Dominio.Entidades
{
    // Valor reutilizavel como variavel de template (Disparos e Agendamentos), cadastrado por
    // empresa. FIXO e um texto que nao muda; CAMPO_CONTATO aponta pra um campo do Contato e e
    // resolvido por destinatario a cada envio (ver ResolvedorDeVariaveis).
    public class Parametro
    {
        public const string Fixo = "FIXO";
        public const string CampoDoContato = "CAMPO_CONTATO";

        public Parametro()
        {
            Id = Guid.NewGuid();
        }

        public Guid Id { get; set; }
        public Guid EmpresaId { get; set; }
        public string Nome { get; set; }
        public string? Descricao { get; set; }
        public string Tipo { get; set; }

        // Texto literal quando Tipo = FIXO; chave do campo do contato (ver
        // ResolvedorDeVariaveis.CamposDoContato) quando Tipo = CAMPO_CONTATO.
        public string Valor { get; set; }

        public DateTime DataCriacao { get; set; }
    }
}
