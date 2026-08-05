using ProjetoMetaMensagem.Dominio.Common;
using ProjetoMetaMensagem.Dominio.Interfaces.Mediator;
using System;

namespace ProjetoMetaMensagem.Dominio.UseCases.Template.ExcluiTemplateConexao
{
    public class ExcluiTemplateConexaoCommand : IRequest<Response<ExcluiTemplateConexaoResult>>
    {
        public ExcluiTemplateConexaoCommand(Guid id)
        {
            Id = id;
        }

        public Guid Id { get; set; }
    }
}
