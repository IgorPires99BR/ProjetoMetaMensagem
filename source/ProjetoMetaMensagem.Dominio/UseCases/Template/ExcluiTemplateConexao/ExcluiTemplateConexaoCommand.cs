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

        // Preenchido pelo controller a partir do JWT (null = administrador, enxerga tudo).
        // Sem esse escopo o DELETE era feito so por Id, e um usuario de outra empresa
        // conseguia apagar conexao alheia mandando o id na rota.
        public Guid? EmpresaIdSolicitante { get; set; }
    }
}
