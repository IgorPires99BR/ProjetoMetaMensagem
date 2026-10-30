using System.Threading.Tasks;

namespace ProjetoMetaMensagem.Dominio.Interfaces.Servicos
{
    public static class ProvedorCobranca
    {
        public const string Asaas = "ASAAS";
        public const string MercadoPago = "MERCADOPAGO";
    }

    // Quem gera, consulta e cancela o Pix das cobrancas aos clientes. Cada CobrancaCliente guarda
    // o provedor que gerou o Pix dela: trocar o ativo (Cobranca:Provedor) nao pode deixar as
    // cobrancas ja enviadas sem baixa. Erros vem como PixException, com mensagem para o usuario.
    public interface IProvedorCobranca
    {
        string Nome { get; }

        // So o Asaas repassa parte do valor para a subconta da empresa. Nos demais o valor
        // inteiro fica na conta da plataforma e o acerto com a empresa e feito por fora.
        bool SuportaSplit { get; }

        Task<PixGerado> CriarCobrancaAsync(NovaCobrancaPix cobranca);

        // Fonte de verdade da baixa: nada e marcado como pago sem passar aqui.
        Task<ConsultaCobrancaPix> ConsultarCobrancaAsync(string idExterno);

        Task CancelarCobrancaAsync(string idExterno);

        // Chamada leve: confirma a credencial sem gerar cobranca.
        Task TestarCredenciaisAsync();
    }

    public interface IProvedoresCobranca
    {
        // O que gera os Pix novos.
        IProvedorCobranca Ativo { get; }

        // Provedor de uma cobranca ja gerada. Nulo = Asaas: as cobrancas anteriores a BD/58 nao
        // gravavam o provedor e todas eram do Asaas.
        IProvedorCobranca Do(string? nome);
    }
}
