using ProjetoMetaMensagem.Dominio.Common;
using ProjetoMetaMensagem.Dominio.Interfaces.Mediator;
using System;

namespace ProjetoMetaMensagem.Dominio.UseCases.Template.AlteraFlagCobrancaTemplate
{
    // Liga/desliga GeraCobranca num template ja existente. Separado de AtualizaTemplate de
    // proposito: aquele reenvia o template pra Meta e so aceita status REJECTED, e essa flag e
    // puramente local -- precisa poder ser ligada num template ja APROVADO, sem tocar na Meta.
    public class AlteraFlagCobrancaTemplateCommand : IRequest<Response<AlteraFlagCobrancaTemplateResult>>
    {
        public Guid TemplateId { get; set; }

        // Escopo sempre vindo do token (nunca do corpo/rota) -- mesmo padrao de DeletaTemplateCommand.
        public Guid? EmpresaIdSolicitante { get; set; }

        public bool GeraCobranca { get; set; }
    }
}
