using System;

namespace ProjetoMetaMensagem.Dominio.UseCases.Cobranca.ProcessaEventoCakto
{
    public class ProcessaEventoCaktoResult
    {
        // O que o evento causou aqui dentro -- aparece no log e ajuda a responder
        // "o cliente pagou, por que a conta dele não abriu?" sem abrir o banco.
        public string Acao { get; set; } = string.Empty;
        public Guid? EmpresaId { get; set; }
        public string? Email { get; set; }
        public bool ContaCriada { get; set; }
    }
}
