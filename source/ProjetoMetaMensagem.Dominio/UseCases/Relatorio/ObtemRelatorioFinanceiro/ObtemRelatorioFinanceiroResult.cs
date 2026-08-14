using ProjetoMetaMensagem.Dominio.Interfaces.Repositorios;
using System.Collections.Generic;

namespace ProjetoMetaMensagem.Dominio.UseCases.Relatorio.ObtemRelatorioFinanceiro
{
    public class ObtemRelatorioFinanceiroResult
    {
        public List<GastoEmpresaMesDto> Gastos { get; set; } = new();
    }
}
