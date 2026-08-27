using System;

namespace ProjetoMetaMensagem.Dominio.UseCases.Empresa.CriaContaCliente
{
    public class CriaContaClienteResult
    {
        public Guid EmpresaId { get; set; }

        public string Email { get; set; } = string.Empty;

        // Devolvida uma unica vez, para quem cadastrou poder passar o acesso na hora por
        // telefone ou WhatsApp, sem depender de o e-mail ter chegado. Nao fica recuperavel
        // depois: o banco guarda so o hash.
        public string SenhaProvisoria { get; set; } = string.Empty;

        public string Mensagem { get; set; } = string.Empty;
    }
}
