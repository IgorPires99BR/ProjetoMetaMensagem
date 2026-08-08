using ProjetoMetaMensagem.Dominio.Common;
using ProjetoMetaMensagem.Dominio.Interfaces.Mediator;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjetoMetaMensagem.Dominio.UseCases.Flows.AlteraFlow
{
    public class AlteraFlowCommand : IRequest<Response<AlteraFlowResult>>
    {
        public Guid Id { get; set; }
        public Guid IdEmpresa { get; set; }
        public string Nome { get; set; } = string.Empty;
        public string? Descricao { get; set; }
        public string? GatilhoPalavraChave { get; set; }
        public bool Ativo { get; set; }

        // Nova estrutura de etapas vinda da edição na tela
        public List<AlteraFlowEtapaDto> Etapas { get; set; } = new List<AlteraFlowEtapaDto>();

        // Preenchido pelo controller a partir do JWT (null = administrador). Nao confundir com
        // IdEmpresa, que vem do corpo e por isso o atacante escolhe. Sem esse escopo o UPDATE
        // casava so pelo Id e permitia reescrever o fluxo de outra empresa.
        public Guid? EmpresaIdSolicitante { get; set; }
    }

    public class AlteraFlowEtapaDto
    {
        public int Ordem { get; set; }
        public string? TipoStep { get; set; } // "Mensagem", "Capturar Input"
        public string? MensagemPergunta { get; set; }
        public string? VariavelSaida { get; set; }
        public Guid? TemplateId { get; set; }
    }
}
