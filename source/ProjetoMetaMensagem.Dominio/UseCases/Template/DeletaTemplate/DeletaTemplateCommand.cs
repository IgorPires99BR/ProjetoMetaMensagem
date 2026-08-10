using ProjetoMetaMensagem.Dominio.Common;
using ProjetoMetaMensagem.Dominio.Interfaces.Mediator;
using System;

namespace ProjetoMetaMensagem.Dominio.UseCases.Template.DeletaTemplate
{
    public class DeletaTemplateCommand : IRequest<Response<DeletaTemplateResult>>
    {
        public Guid TemplateId { get; set; }

        // Escopo sempre vindo do token (nunca do corpo/rota) -- mesmo padrao de ExcluiTemplateConexaoCommand
        public Guid? EmpresaIdSolicitante { get; set; }
    }
}
