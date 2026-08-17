using System;

namespace ProjetoMetaMensagem.Dominio.Entidades
{
    // Situação comercial da empresa, alimentada pelos eventos da Cakto.
    public static class StatusAssinatura
    {
        public const string Ativa = "ATIVA";
        public const string Inadimplente = "INADIMPLENTE";
        public const string Cancelada = "CANCELADA";
        public const string Reembolsada = "REEMBOLSADA";
    }

    public static class PlanoAssinatura
    {
        public const string Starter = "STARTER";
        public const string Pro = "PRO";
        public const string Enterprise = "ENTERPRISE";
    }

    public class Assinatura
    {
        public Guid Id { get; set; }
        public Guid EmpresaId { get; set; }

        // Identificadores do lado da Cakto
        public string? AssinaturaIdCakto { get; set; }
        public string? ClienteIdCakto { get; set; }
        public string? OfertaIdCakto { get; set; }
        public string? EmailComprador { get; set; }

        public string Plano { get; set; } = PlanoAssinatura.Starter;
        public string Status { get; set; } = StatusAssinatura.Ativa;
        public int? ValorCentavos { get; set; }

        public DateTime DataInicio { get; set; }
        public DateTime? DataProximaCobranca { get; set; }
        public DateTime? DataCancelamento { get; set; }

        // Ultimo evento aplicado, para diagnostico ("por que esta conta foi suspensa?")
        public string? EventoIdCakto { get; set; }
        public string? UltimoEvento { get; set; }
        public DateTime? DataUltimoEvento { get; set; }

        // De onde veio esta venda (UTMs do checkout + identificadores do Facebook)
        public string? UtmSource { get; set; }
        public string? UtmMedium { get; set; }
        public string? UtmCampaign { get; set; }
        public string? UtmTerm { get; set; }
        public string? UtmContent { get; set; }
        public string? Sck { get; set; }
        public string? Fbc { get; set; }
        public string? Fbp { get; set; }
        public string? RefIdCakto { get; set; }
        public string? MetodoPagamento { get; set; }

        public DateTime DataCriacao { get; set; }
        public DateTime? DataAtualizacao { get; set; }

        public Assinatura()
        {
            Id = Guid.NewGuid();
            DataCriacao = DateTime.Now;
            DataInicio = DateTime.Now;
        }

        // Só ATIVA libera a plataforma. Inadimplente/cancelada/reembolsada mantêm o acesso de
        // leitura (o cliente ainda entra e vê os dados dele), mas não deixam enviar mensagem --
        // a regra fica em quem envia, não aqui.
        public bool PermiteEnvio() => Status == StatusAssinatura.Ativa;
    }

    // Evento cru recebido da Cakto, guardado para idempotência e auditoria.
    public class EventoCakto
    {
        public Guid Id { get; set; }
        public string EventoIdCakto { get; set; } = string.Empty;
        public string Evento { get; set; } = string.Empty;
        public Guid? EmpresaId { get; set; }
        public string? PayloadJson { get; set; }
        public DateTime DataRecebido { get; set; }

        public EventoCakto()
        {
            Id = Guid.NewGuid();
            DataRecebido = DateTime.Now;
        }
    }
}
