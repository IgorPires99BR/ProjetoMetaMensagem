namespace ProjetoMetaMensagem.Dominio.Entidades
{
    // Origem de uma variavel {{n}} do corpo do template num Agendamento: "nome"/"telefone"
    // resolvido por contato a cada disparo recorrente, ou "fixo" (ValorFixo) igual pra todos.
    // Mesmo desenho da tela de Disparo em lote, persistido para repetir em cada execucao.
    public class AgendamentoVariavelDto
    {
        public string Origem { get; set; }
        public string ValorFixo { get; set; }
    }
}
