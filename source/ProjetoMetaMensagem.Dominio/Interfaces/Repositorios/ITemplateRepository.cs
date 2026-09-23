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

        // Update dedicado e minimo (so a coluna local GeraCobranca) -- nao passa por Alterar()
        // porque esse reenvia o template pra Meta e exige status REJECTED, e ligar/desligar
        // cobranca e uma flag puramente nossa, sem relacao com a analise do template.
        Task<int> AlterarFlagCobranca(Guid id, Guid? empresaIdSolicitante, bool geraCobranca);
    }
}
