using System;

namespace ProjetoMetaMensagem.Dominio.UseCases.Numero.AtivaCoexistencia
{
    public class AtivaCoexistenciaResult
    {
        public Guid NumeroId { get; set; }
        public string StatusConexao { get; set; }
    }
}
