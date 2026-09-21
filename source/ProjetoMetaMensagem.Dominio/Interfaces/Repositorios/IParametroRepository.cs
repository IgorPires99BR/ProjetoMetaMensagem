using ProjetoMetaMensagem.Dominio.Entidades;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ProjetoMetaMensagem.Dominio.Interfaces.Repositorios
{
    public interface IParametroRepository
    {
        Task Incluir(Parametro parametro);
        Task<int> Alterar(Parametro parametro, Guid? empresaIdSolicitante);
        Task<int> Excluir(Guid id, Guid? empresaIdSolicitante);
        Task<IEnumerable<Parametro>> ObterPorEmpresa(Guid empresaId);

        // Quantos agendamentos referenciam o parametro nas variaveis (VariaveisJson) -- excluir
        // um parametro em uso deixaria a variavel vazia e a Meta recusaria o disparo.
        Task<int> ContarAgendamentosUsando(Guid parametroId);
    }
}
