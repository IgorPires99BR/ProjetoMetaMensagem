using ProjetoMetaMensagem.Dominio.Entidades;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ProjetoMetaMensagem.Dominio.Interfaces.Repositorios
{
    public interface IAssinaturaRepository
    {
        Task Incluir(Assinatura assinatura);
        Task<int> Alterar(Assinatura assinatura);

        Task<Assinatura?> ObterPorEmpresa(Guid empresaId);
        Task<Assinatura?> ObterPorAssinaturaCakto(string assinaturaIdCakto);
        Task<Assinatura?> ObterPorEmailComprador(string email);
        Task<IEnumerable<Assinatura>> Listar();

        // Idempotência do webhook: a Cakto reenvia o mesmo evento até 5 vezes.
        Task<bool> EventoJaProcessado(string eventoIdCakto, string evento);
        Task RegistrarEvento(EventoCakto evento);
    }
}
