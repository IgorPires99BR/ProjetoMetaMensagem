using ProjetoMetaMensagem.Dominio.Common;
using ProjetoMetaMensagem.Dominio.Interfaces.Mediator;
using System;

namespace ProjetoMetaMensagem.Dominio.UseCases.Template.AlteraAtivoTemplate
{
    // Inativa ou reativa o template so aqui no sistema. Separado de DeletaTemplate de proposito:
    // aquele apaga tambem na Meta, o que nao tem volta e impediria reativar.
    public class AlteraAtivoTemplateCommand : IRequest<Response<AlteraAtivoTemplateResult>>
    {
        public Guid TemplateId { get; set; }

        // Escopo sempre vindo do token (nunca do corpo/rota) -- mesmo padrao de DeletaTemplateCommand.
        public Guid? EmpresaIdSolicitante { get; set; }

        public bool Ativo { get; set; }
    }
}
