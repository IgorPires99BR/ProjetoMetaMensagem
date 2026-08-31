using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjetoMetaMensagem.Dominio.UseCases.Contato.ObtemContato
{
    public class ObtemContatoResult
    {
        public ObtemContatoResult(Entidades.Contato contato, Entidades.OrigemLead? origem = null)
        {
            Id = contato.Id;
            UsuarioId = contato.UsuarioId;
            Telefone = contato.Telefone;
            Nome = contato.Nome;
            Email = contato.Email;
            DataCriacao = contato.DataCriacao;

            // Origem gravada na primeira mensagem de quem chegou por um anuncio Click-to-
            // WhatsApp (OrigemLead), mas nunca lida fora do momento da compra -- ate agora
            // ninguem via de qual anuncio um lead vinha. Null pra quem foi cadastrado
            // manualmente ou escreveu organicamente, sem passar por anuncio nenhum.
            OrigemAnuncio = origem?.Headline ?? origem?.SourceId;
            OrigemData = origem?.DataPrimeiroContato;
        }
        public Guid Id { get; set; }
        public Guid UsuarioId { get; set; }
        public string Telefone { get; set; }
        public string? Nome { get; set; }
        public string? Email { get; set; }
        public DateTime DataCriacao { get; set; }
        public string? OrigemAnuncio { get; set; }
        public DateTime? OrigemData { get; set; }
    }
}
