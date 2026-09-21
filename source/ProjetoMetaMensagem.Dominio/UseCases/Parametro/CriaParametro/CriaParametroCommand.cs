using ProjetoMetaMensagem.Dominio.Common;
using ProjetoMetaMensagem.Dominio.Interfaces.Mediator;
using System;

namespace ProjetoMetaMensagem.Dominio.UseCases.Parametro.CriaParametro
{
    public class CriaParametroCommand : IRequest<Response<CriaParametroResult>>
    {
        public Guid EmpresaId { get; set; }
        public string Nome { get; set; }
        public string? Descricao { get; set; }

        // FIXO ou CAMPO_CONTATO (ver Entidades.Parametro).
        public string Tipo { get; set; }
        public string Valor { get; set; }
    }
}
