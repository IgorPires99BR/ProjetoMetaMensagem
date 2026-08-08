using ProjetoMetaMensagem.Dominio.Entidades;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjetoMetaMensagem.Dominio.UseCases.Numero.ListarNumeros
{
    public class ListarNumerosResult
    {
        public ListarNumerosResult(Entidades.Numero numero)
        {
            Id = numero.Id;
            UsuarioId = numero.UsuarioId;
            Telefone = numero.Telefone;
            Descricao = numero.Descricao;
            InstanciaId = numero.InstanciaId;
            // Estes quatro existiam como propriedade mas nunca eram preenchidos: a API devolvia
            // sempre null e a tela caia no fallback, mostrando TODO numero como "CONNECTED" e os
            // contadores de status zerados -- contradizendo o dashboard, que le direto do banco.
            StatusMeta = numero.StatusMeta;
            QualidadeMeta = numero.QualidadeMeta;
            // A tela compara com o numero do enum (2 = Coexistencia), entao sai como int.
            TipoConexao = (int)numero.TipoConexao;
            StatusConexao = numero.StatusConexao;
            DataCriacao = numero.DataCriacao;
            DataAtualizacao = numero.DataAtualizacao;
        }

        public Guid Id { get; set; }
        public Guid UsuarioId { get; set; }
        public string Telefone { get; set; }
        public string? Descricao { get; set; }
        public string? InstanciaId { get; set; }
        public string? StatusMeta { get; set; }
        public string? QualidadeMeta { get; set; }
        // Usados pela tela pra marcar o numero em Coexistencia e esconder o botao de ativar.
        // O SystemUserToken da entidade NAO entra aqui de proposito: e credencial.
        public int TipoConexao { get; set; }
        public string? StatusConexao { get; set; }
        public DateTime DataCriacao { get; set; }
        public DateTime? DataAtualizacao { get; set; }
    }
}
