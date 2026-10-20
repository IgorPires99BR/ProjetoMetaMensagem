using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace ProjetoMetaMensagem.Dominio.Entidades
{
    // Momento em que um template e disparado sozinho, sem ninguem apertar "enviar" (ver BD/55 e
    // DisparoPorGatilho). Um gatilho de cada tipo por template.
    public class TemplateGatilho
    {
        public const string CadastroContato = "CADASTRO_CONTATO";
        // Cobranca (CobrancaCliente) ainda em aberto DiasAtraso dias depois do vencimento. Sem
        // cobranca gerada nao ha como saber se o contato pagou, entao so vale para quem usa
        // template com "gera cobranca".
        public const string AtrasoPagamento = "ATRASO_PAGAMENTO";

        public static readonly string[] Tipos = { CadastroContato, AtrasoPagamento };

        public TemplateGatilho()
        {
            Id = Guid.NewGuid();
            Ativo = true;
            DataCriacao = DateTime.Now;
        }

        public Guid Id { get; set; }
        public Guid EmpresaId { get; set; }
        public Guid TemplateId { get; set; }
        public string Tipo { get; set; } = CadastroContato;
        public int? DiasAtraso { get; set; }
        public string? VariaveisJson { get; set; }
        public bool Ativo { get; set; }
        public DateTime DataCriacao { get; set; }
        public DateTime? DataAtualizacao { get; set; }

        [NotMapped]
        public List<AgendamentoVariavelDto> Variaveis
        {
            get => string.IsNullOrEmpty(VariaveisJson)
                ? new List<AgendamentoVariavelDto>()
                : JsonConvert.DeserializeObject<List<AgendamentoVariavelDto>>(VariaveisJson) ?? new List<AgendamentoVariavelDto>();
            set => VariaveisJson = JsonConvert.SerializeObject(value ?? new List<AgendamentoVariavelDto>());
        }
    }

    // Cobranca em atraso que um gatilho ainda nao avisou (consulta do job de atraso).
    public class CobrancaEmAtraso
    {
        public Guid GatilhoId { get; set; }
        public Guid EmpresaId { get; set; }
        public Guid CobrancaClienteId { get; set; }
        public Guid ContatoId { get; set; }
    }
}
