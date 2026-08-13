using ProjetoMetaMensagem.Dominio.Entidades;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ProjetoMetaMensagem.Dominio.Interfaces.Repositorios
{
    public interface ICampanhaRepository
    {
        Task<Guid> Incluir(Campanha campanha);
        Task IncluirContatos(List<CampanhaContato> contatos);
        Task<IEnumerable<Campanha>> Listar(Guid empresaId);
        Task<IEnumerable<Campanha>> ObterPendentes();
        // empresaIdSolicitante restringe a operacao as campanhas da empresa informada.
        // null = administrador ou processo de fundo (sem restricao). Campanha tem EmpresaId proprio.
        Task<int> AtualizarStatus(Guid id, string status, Guid? empresaIdSolicitante);
        Task<Campanha?> ObterPorId(Guid id);
        Task<IEnumerable<CampanhaContato>> ObterContatosPorCampanha(Guid campanhaId);
        // Marca o contato como tratado antes do envio. Devolve false se ele ja estava tratado
        // (outro worker pegou, ou e retomada de campanha interrompida) -- nesse caso, nao enviar.
        Task<bool> ReivindicarContato(Guid vinculoId);
        // Resultado por contato (entregue/falhou e o motivo). O worker preenchia esses campos
        // so no objeto em memoria e nunca gravava, entao o relatorio da campanha ficava vazio.
        Task AtualizarResultadoContato(CampanhaContato vinculo);
        // Contador de processados da campanha, pela mesma razao acima.
        Task AtualizarProgresso(Guid campanhaId, int processados);
    }
}
