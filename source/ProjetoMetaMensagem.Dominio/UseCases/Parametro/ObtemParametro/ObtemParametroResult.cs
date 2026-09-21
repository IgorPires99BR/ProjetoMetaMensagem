using System;

namespace ProjetoMetaMensagem.Dominio.UseCases.Parametro.ObtemParametro
{
    public class ObtemParametroResult
    {
        public ObtemParametroResult()
        {
        }

        public ObtemParametroResult(Entidades.Parametro parametro)
        {
            Id = parametro.Id;
            EmpresaId = parametro.EmpresaId;
            Nome = parametro.Nome;
            Descricao = parametro.Descricao;
            Tipo = parametro.Tipo;
            Valor = parametro.Valor;
        }

        public Guid Id { get; set; }
        public Guid EmpresaId { get; set; }
        public string Nome { get; set; }
        public string? Descricao { get; set; }
        public string Tipo { get; set; }
        public string Valor { get; set; }
    }
}
