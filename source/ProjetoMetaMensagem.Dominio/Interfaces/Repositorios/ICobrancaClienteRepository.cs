using ProjetoMetaMensagem.Dominio.Entidades;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ProjetoMetaMensagem.Dominio.Interfaces.Repositorios
{
    public interface ICobrancaClienteRepository
    {
        Task Incluir(CobrancaCliente cobranca);

        // empresaIdSolicitante restringe a busca/operacao a empresa informada. null = admin da
        // plataforma (sem restricao) -- mesmo padrao do ITemplateRepository/IContatoRepository.
        Task<CobrancaCliente?> ObterPorId(Guid id, Guid? empresaIdSolicitante);

        // status nulo = todas. Ordenado por vencimento: e a lista que a tela de gestao e o
        // futuro job de recorrencia (cobrancas vencidas e nao pagas) vao consumir.
        // dataInicio/dataFim filtram pela data de envio da cobranca (DataCriacao); nulos = sem limite.
        Task<IEnumerable<CobrancaCliente>> ObterPorEmpresa(Guid? empresaIdSolicitante, string? status,
            DateTime? dataInicio = null, DateTime? dataFim = null);

        Task<int> MarcarPaga(Guid id, Guid? empresaIdSolicitante, DateTime dataPagamento);
    }
}
