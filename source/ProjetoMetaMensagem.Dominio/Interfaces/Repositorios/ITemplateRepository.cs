using ProjetoMetaMensagem.Dominio.Entidades;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjetoMetaMensagem.Dominio.Interfaces.Repositorios
{
    public interface ITemplateRepository
    {
        Task Incluir(Template template);
        // empresaIdSolicitante restringe a operacao aos templates da empresa informada.
        // null = administrador (sem restricao). Template tem EmpresaId proprio.
        Task<int> Alterar(Template template, Guid? empresaIdSolicitante);
        Task<int> Excluir(Guid id, Guid? empresaIdSolicitante);
        Task<Template?> ObterPorId(int id);
        // empresaIdSolicitante restringe a busca aos templates da empresa informada (null = admin, sem restricao)
        Task<Template?> ObterPorIdEEmpresa(Guid id, Guid? empresaIdSolicitante);
        Task<IEnumerable<Template>> Obter();
        Task<IEnumerable<Template>> ObterPorEmpresa(Guid empresaId);

        // Templates das OUTRAS empresas que dividem o mesmo WABA (ex: Sebrecon usa o numero da
        // Contact Solution). A Meta lista tudo do WABA junto, entao a sincronizacao precisa
        // disso pra nao importar como seu um template que outra empresa criou.
        Task<IEnumerable<Template>> ObterDeOutrasEmpresasDoWaba(Guid empresaId, string wabaId);

        // Update dedicado e minimo (so a coluna local GeraCobranca) -- nao passa por Alterar()
        // porque esse reenvia o template pra Meta e exige status REJECTED, e ligar/desligar
        // cobranca e uma flag puramente nossa, sem relacao com a analise do template.
        Task<int> AlterarFlagCobranca(Guid id, Guid? empresaIdSolicitante, bool geraCobranca);

        // Gatilhos de disparo automatico (BD/55). Ficam aqui e nao num repositorio proprio porque
        // so existem pendurados num template.
        Task<IEnumerable<TemplateGatilho>> ObterGatilhosPorEmpresa(Guid empresaId);
        Task<IEnumerable<TemplateGatilho>> ObterGatilhosAtivos(Guid empresaId, string tipo);
        // Troca todos os gatilhos do template pelos informados (lista vazia = remove todos).
        Task SubstituirGatilhos(Guid templateId, Guid empresaId, IEnumerable<TemplateGatilho> gatilhos);

        // Cobrancas em aberto que ja passaram dos DiasAtraso de algum gatilho ativo e que esse
        // gatilho ainda nao avisou. So entra cobranca que atingiu o atraso depois da criacao do
        // gatilho: ligar um gatilho hoje nao pode disparar para meses de cobrancas antigas.
        Task<IEnumerable<CobrancaEmAtraso>> ObterCobrancasEmAtrasoSemAviso(DateTime hoje);
        Task RegistrarEnvioGatilho(Guid gatilhoId, Guid cobrancaClienteId);
    }
}
