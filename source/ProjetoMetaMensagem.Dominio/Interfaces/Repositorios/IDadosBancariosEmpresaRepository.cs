using ProjetoMetaMensagem.Dominio.Entidades;
using System;
using System.Threading.Tasks;

namespace ProjetoMetaMensagem.Dominio.Interfaces.Repositorios
{
    public interface IDadosBancariosEmpresaRepository
    {
        Task<DadosBancariosEmpresa?> ObterPorEmpresa(Guid empresaId);
        // Insere ou atualiza: e uma linha por empresa, criada no primeiro salvamento.
        Task Salvar(DadosBancariosEmpresa dados);
    }
}
