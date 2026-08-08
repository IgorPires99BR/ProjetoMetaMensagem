using ProjetoMetaMensagem.Dominio.Entidades;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjetoMetaMensagem.Dominio.Interfaces.Repositorios
{
    public interface IUsuarioRepository
    {
        Task Incluir(Usuario usuario);
        // empresaIdSolicitante restringe a operacao aos usuarios da empresa informada.
        // null = administrador (sem restricao) ou fluxo sem usuario logado, como a
        // redefinicao de senha. Usuario tem EmpresaId proprio.
        // O escopo e obrigatorio de proposito (sem valor padrao): um default nulo deixaria
        // um chamador futuro esquecer o recorte por empresa sem nenhum erro de compilacao,
        // reabrindo em silencio o furo que permitia alterar registro de outro tenant.
        // Passe null explicitamente quando nao houver usuario logado (processo de fundo).
        Task<int> Alterar(Usuario usuario, Guid? empresaIdSolicitante);

        Task<Usuario?> ObterPorEmail(string email);
        Task<Usuario?> Logar(string email, string senhaHash);

        Task<int> Excluir(string id, Guid? empresaIdSolicitante);
        Task<Usuario?> ObterPorId(Guid id);
        Task<IEnumerable<Usuario>> Obter();
        // Método adicional comum para usuários
        Task<IEnumerable<Usuario>> ObterPorEmpresa(Guid empresaId);
    }
}
