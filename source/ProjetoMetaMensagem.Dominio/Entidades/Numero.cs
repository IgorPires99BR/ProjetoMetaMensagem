using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ProjetoMetaMensagem.Dominio.UseCases.Numero.CriaNumero;
using ProjetoMetaMensagem.Dominio.UseCases.Numero.AlteraNumero;
using ProjetoMetaMensagem.Dominio.Enums;

namespace ProjetoMetaMensagem.Dominio.Entidades
{
    public class Numero
    {

        public Numero()
        {
            
        }

        public Numero(CriaNumeroCommand command)
        {
            UsuarioId = command.UsuarioId;
            Telefone = command.NumeroTelefone;
            Descricao = command.NomeEmpresa;
            DataCriacao = DateTime.Now;
        }

        public Numero(AlteraNumeroCommand command)
        {
            Id = command.Id;
            UsuarioId = command.UsuarioId;
            Telefone = command.NumeroTelefone;
            Descricao = command.Descricao;
            InstanciaId = command.InstanciaId;
            DataAtualizacao = DateTime.Now;
        }

        [Key]
        public Guid Id { get; set; }

        [Required]
        public Guid UsuarioId { get; set; }

        [ForeignKey("UsuarioId")]
        public Usuario Usuario { get; set; }

        [Required]
        [MaxLength(50)]
        public string Telefone { get; set; }

        [MaxLength(100)]
        public string? Descricao { get; set; }

        [MaxLength(255)]
        public string? InstanciaId { get; set; }

        public string? StatusMeta { get; set; }

        public string? QualidadeMeta { get; set; }
        public DateTime DataCriacao { get; set; }
        public DateTime? DataAtualizacao { get; set; }

        public TipoConexaoNumero TipoConexao { get; set; } = TipoConexaoNumero.ApiOficial;
        public string? StatusConexao { get; set; }
        public string? SystemUserToken { get; set; }

        [MaxLength(100)]
        public string? WabaId { get; set; }
        public DateTime? DataUltimaSincronizacao { get; set; }

        // Quando o SystemUserToken foi trocado por long-lived (~60 dias), guarda a validade
        // devolvida pela Meta. NULL = token sem expiracao conhecida (ex: numeros antigos,
        // cadastrados antes dessa troca existir).
        public DateTime? TokenExpiraEm { get; set; }
    }
}
