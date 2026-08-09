namespace ProjetoMetaMensagem.Dominio.Interfaces.Servicos.Meta
{
    public class ResultadoCoexistencia
    {
        public bool Sucesso { get; set; }
        public string StatusConexao { get; set; }
        public string Erro { get; set; }
    }
}
