using ProjetoMetaMensagem.Dominio.Common;
using ProjetoMetaMensagem.Dominio.Interfaces.Mediator;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjetoMetaMensagem.Dominio.UseCases.Flows.CriaFlow
{
    public class CriaFlowCommand : IRequest<Response<CriaFlowResult>>
    {
        public Guid IdEmpresa { get; set; }
        public string Nome { get; set; } = string.Empty;
        public string? Descricao { get; set; }
        public string? GatilhoPalavraChave { get; set; } // Representa o "oi, olá, bom dia" da imagem

        // Lista com os steps que o usuário montou na tela
        public List<CriaFlowEtapaDto> Etapas { get; set; } = new List<CriaFlowEtapaDto>();
    }

    public class CriaFlowEtapaDto
    {
        public int Ordem { get; set; } // Ajuda a identificar a sequência vinda do front
        public string? TipoStep { get; set; } // "Mensagem", "Capturar Input", etc.
        public string? MensagemPergunta { get; set; } // "Olá! Bem-vindo..." ou "Qual seu nome?"
        public string? VariavelSaida { get; set; } // "nome" (como no step 2 da imagem), so se aplica a "Capturar Input"
        public bool EhEtapaInicial { get; set; }
        public Guid? TemplateId { get; set; } // Template que dispara o inicio do flow (quando aplicavel)
    }
}
