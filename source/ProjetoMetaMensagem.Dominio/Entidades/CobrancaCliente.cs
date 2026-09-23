using System;

namespace ProjetoMetaMensagem.Dominio.Entidades
{
    // Status da cobranca que quem revende a plataforma (ex: Sebrecon) faz ao PROPRIO cliente
    // dela (ex: um devedor do Gilson). Nao confundir com StatusAssinatura (Assinatura.cs), que
    // e a cobranca da assinatura SaaS da propria Contact Solution.
    public static class StatusCobrancaCliente
    {
        public const string Pendente = "PENDENTE";
        public const string Paga = "PAGA";
        public const string Vencida = "VENCIDA";
        public const string Cancelada = "CANCELADA";
    }

    // Uma linha por disparo de Template com GeraCobranca = true: a cobranca do cliente final de
    // quem revende a plataforma, aberta no momento do envio (ver CobrancaClienteFactory).
    //
    // Valor/DataVencimento sao snapshot do Contato no disparo -- nao mudam se o cadastro do
    // contato for alterado depois, porque a cobranca ja foi enviada com aqueles valores.
    public class CobrancaCliente
    {
        public CobrancaCliente()
        {
            Id = Guid.NewGuid();
            Status = StatusCobrancaCliente.Pendente;
            DataCriacao = DateTime.Now;
        }

        public Guid Id { get; set; }
        public Guid EmpresaId { get; set; }
        public Guid ContatoId { get; set; }
        public Guid TemplateId { get; set; }
        public Guid HistoricoDisparoId { get; set; }

        public decimal Valor { get; set; }
        public DateTime DataVencimento { get; set; }

        public string Status { get; set; } = StatusCobrancaCliente.Pendente;
        public DateTime? DataPagamento { get; set; }

        // Chave de correlacao com o webhook da Cakto (utm_content do link de pagamento) -- sem
        // uso ainda, porque o link hoje e o mesmo checkout compartilhado da Contact Solution.
        // Confirmacao de pagamento por enquanto e manual (ver MarcaCobrancaClientePaga).
        public string? UtmContentCakto { get; set; }
        public string? EventoIdCakto { get; set; }

        public DateTime DataCriacao { get; set; }
        public DateTime? DataAtualizacao { get; set; }
    }
}
