using ProjetoMetaMensagem.Dominio.Interfaces.Servicos.Meta;
using ProjetoMetaMensagem.Dominio.UseCases.Numero.Common;
using System;
using System.Collections.Generic;

namespace ProjetoMetaMensagem.Dominio.UseCases.Numero.ObtemPerfilNumero
{
    public class ObtemPerfilNumeroResult
    {
        public ObtemPerfilNumeroResult(Entidades.Numero numero, PerfilNumeroMetaDto perfil)
        {
            NumeroId = numero.Id;
            Telefone = numero.Telefone;
            EhCoexistencia = NumeroMetaAcesso.EhCoexistencia(numero);
            Sobre = perfil.Sobre;
            Descricao = perfil.Descricao;
            Endereco = perfil.Endereco;
            Email = perfil.Email;
            Sites = perfil.Sites;
            Segmento = perfil.Segmento;
            FotoUrl = perfil.FotoUrl;
            NomeExibido = perfil.NomeExibido;
            NovoNomeSolicitado = perfil.NovoNomeSolicitado;
            StatusNovoNome = perfil.StatusNovoNome;
        }

        public Guid NumeroId { get; set; }
        public string Telefone { get; set; }
        public bool EhCoexistencia { get; set; }
        public string? Sobre { get; set; }
        public string? Descricao { get; set; }
        public string? Endereco { get; set; }
        public string? Email { get; set; }
        public List<string> Sites { get; set; }
        public string? Segmento { get; set; }
        public string? FotoUrl { get; set; }
        public string? NomeExibido { get; set; }
        public string? NovoNomeSolicitado { get; set; }

        // PENDING_REVIEW, APPROVED, DECLINED... (texto da Meta, sem tradução)
        public string? StatusNovoNome { get; set; }
    }
}
