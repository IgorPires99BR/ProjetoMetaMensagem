using ProjetoMetaMensagem.Dominio.Common;
using ProjetoMetaMensagem.Dominio.Interfaces.Mediator;
using System;

namespace ProjetoMetaMensagem.Dominio.UseCases.Parametro.AlteraParametro
{
    public class AlteraParametroCommand : IRequest<Response<AlteraParametroResult>>
    {
        public Guid Id { get; set; }
        public string Nome { get; set; }
        public string? Descricao { get; set; }
        public string Tipo { get; set; }
        public string Valor { get; set; }

        // Preenchido pelo controller a partir do JWT (null = administrador de plataforma).
        public Guid? EmpresaIdSolicitante { get; set; }
    }
}
