namespace ProjetoMetaMensagem.Dominio.Interfaces.Servicos.Meta
{
    public class ResultadoEnvioTemplate
    {
        public bool Sucesso { get; set; }
        public string WamidMeta { get; set; }
        public string Erro { get; set; }
        public string? JsonEnviado { get; set; }
        public string ContatoId { get; set; }
    }
}
