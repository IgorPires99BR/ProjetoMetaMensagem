using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjetoMetaMensagem.Dominio.UseCases.Messages.EnviarMensagemTemplateMetaLote
{
    public class EnviarMensagemTemplateMetaLoteResult
    {
        public Dictionary<string, bool> RelatorioDisparos { get; set; } = new Dictionary<string, bool>();
        public Dictionary<string, string> RelatorioErros { get; set; } = new Dictionary<string, string>();
        public int TotalProcessado => RelatorioDisparos?.Count ?? 0;
        public int TotalSucesso => RelatorioDisparos?.Count(x => x.Value) ?? 0;
        public int TotalFalha => RelatorioDisparos?.Count(x => !x.Value) ?? 0;
    }
}
