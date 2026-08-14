using ProjetoMetaMensagem.Dominio.Interfaces.Repositorios;
using System.Collections.Generic;

namespace ProjetoMetaMensagem.Dominio.UseCases.Relatorio.ObtemRelatorioEngajamento
{
    public class EngajamentoEmpresaResultDto
    {
        public System.Guid EmpresaId { get; set; }
        public string NomeEmpresa { get; set; } = string.Empty;
        public int Enviados { get; set; }
        public int Visualizaram { get; set; }
        public int Responderam { get; set; }
        public int NaoResponderam { get; set; }
    }

    public class ObtemRelatorioEngajamentoResult
    {
        public List<EngajamentoEmpresaResultDto> Empresas { get; set; } = new();
    }
}
