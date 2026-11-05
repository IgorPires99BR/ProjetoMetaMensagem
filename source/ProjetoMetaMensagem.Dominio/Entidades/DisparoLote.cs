using System;
using System.Collections.Generic;

namespace ProjetoMetaMensagem.Dominio.Entidades
{
    public static class StatusDisparoItem
    {
        public const string Pendente = "PENDENTE";
        // Reservado por um worker, gerando o Pix. Nada saiu ainda: queda aqui pode repetir.
        public const string Preparando = "PREPARANDO";
        // Gravado ANTES de chamar a Meta. Item que ficar preso aqui pode ter sido enviado, entao
        // nunca volta para a fila: vira INCERTO.
        public const string Enviando = "ENVIANDO";
        public const string Enviado = "ENVIADO";
        public const string Falhou = "FALHOU";
        // A Meta nao confirmou nem negou. Cobranca fica registrada e quem decide e o usuario.
        public const string Incerto = "INCERTO";
    }

    // Um pedido de disparo em massa da tela de Disparos (ver BD/62 e ProcessadorDisparoLote).
    public class DisparoLote
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public Guid EmpresaId { get; set; }
        public Guid TemplateId { get; set; }
        public Guid ChaveIdempotencia { get; set; }
        public string? VariaveisJson { get; set; }
        public string? ParametroHeaderMediaUrl { get; set; }
        public string? ParametrosButtonJson { get; set; }
        public DateTime DataCriacao { get; set; }
    }

    // Um contato do lote. Telefone2 nao vira outro item: a fatura e uma so por contato.
    public class DisparoLoteItem
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public Guid LoteId { get; set; }
        public Guid ContatoId { get; set; }
        public int Ordem { get; set; }
        public string Status { get; set; } = StatusDisparoItem.Pendente;
        public int Tentativas { get; set; }
        public string? Erro { get; set; }
        public string? EnvioJson { get; set; }
        public string? CobrancaJson { get; set; }
    }

    public class DisparoLoteResumo
    {
        public Guid Id { get; set; }
        public DateTime DataCriacao { get; set; }
        public string? NomeTemplate { get; set; }
        public int Total { get; set; }
        public int Aguardando { get; set; }
        public int Enviados { get; set; }
        public int Falhas { get; set; }
        public int Incertos { get; set; }
        public bool Concluido => Aguardando == 0;

        // Quem precisa de atencao. So vem na consulta de um lote especifico.
        public List<DisparoLotePendencia> Pendencias { get; set; } = new();
    }

    public class DisparoLotePendencia
    {
        public Guid ContatoId { get; set; }
        public string? Nome { get; set; }
        public string? Telefone { get; set; }
        public string Status { get; set; } = string.Empty;
        public string? Erro { get; set; }
    }
}
