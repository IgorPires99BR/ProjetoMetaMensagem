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
        // apenasInativas troca a lista das ativas pela das inativas (aba da tela).
        Task<IEnumerable<CobrancaCliente>> ObterPorEmpresa(Guid? empresaIdSolicitante, string? status,
            DateTime? dataInicio = null, DateTime? dataFim = null, bool apenasInativas = false);

        Task<int> MarcarPaga(Guid id, Guid? empresaIdSolicitante, DateTime dataPagamento);

        // Baixa automatica do Pix (webhook/consulta periodica do Asaas). Sem recorte de empresa:
        // quem chama ja confirmou o pagamento reconsultando o Asaas.
        Task<CobrancaCliente?> ObterPorTxid(string txid);
        Task<IEnumerable<CobrancaCliente>> ObterPendentesComPix(DateTime expiradoDesde);
        Task<int> BaixarPix(Guid id, DateTime dataPagamento, decimal valorPago, string? endToEndId);

        // Pix novo na mesma cobranca (recorrencia com o anterior vencido). Devolve 0 se ela deixou
        // de estar pendente.
        Task<int> AtualizarPix(CobrancaCliente cobranca);

        // Recorrencia enviada: a cobranca passa a valer como vencida, mesmo antes da data.
        Task<int> MarcarVencida(Guid id);

        // Devolvem 0 se a cobranca nao existe no escopo ou ja esta na situacao pedida.
        Task<int> Inativar(Guid id, Guid? empresaIdSolicitante, DateTime agora);
        Task<int> Reativar(Guid id, Guid? empresaIdSolicitante);
    }
}
