using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjetoMetaMensagem.Dominio.UseCases.Numero.ObtemNumero
{
    public class ObtemNumeroResult
    {
        public ObtemNumeroResult(Entidades.Numero numero)
        {
            Id = numero.Id;
            UsuarioId = numero.UsuarioId;
            Telefone = numero.Telefone;
            Descricao = numero.Descricao;
            InstanciaId = numero.InstanciaId;
            DataCriacao = numero.DataCriacao;
            DataAtualizacao = numero.DataAtualizacao;
        }

        public Guid Id { get; set; }
        public Guid UsuarioId { get; set; }
        public string Telefone { get; set; }
        public string? Descricao { get; set; }
        public string? InstanciaId { get; set; }
        public DateTime DataCriacao { get; set; }
        public DateTime? DataAtualizacao { get; set; }
    }
}
