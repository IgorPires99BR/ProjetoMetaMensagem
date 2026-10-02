using System;

namespace ProjetoMetaMensagem.Dominio.UseCases.Numero.SolicitaNomeNumero
{
    public class SolicitaNomeNumeroResult
    {
        public Guid NumeroId { get; set; }
        public string NovoNome { get; set; }
    }
}
