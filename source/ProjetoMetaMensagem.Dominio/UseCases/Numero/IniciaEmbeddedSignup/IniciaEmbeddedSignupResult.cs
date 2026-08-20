using System;

namespace ProjetoMetaMensagem.Dominio.UseCases.Numero.IniciaEmbeddedSignup
{
    public class IniciaEmbeddedSignupResult
    {
        public Guid NumeroId { get; set; }
        public string StatusConexao { get; set; }

        // false = numero foi cadastrado, mas a assinatura do app no WABA falhou -- o cliente
        // fica "conectado" sem receber mensagens ate uma sincronizacao bem sucedida. O front
        // deve avisar o usuario a tentar "Sincronizar Meta" novamente nesse caso.
        public bool AppAssinado { get; set; }
    }
}
