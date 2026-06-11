using ProjetoMetaMensagem.Dominio.Entidades;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjetoMetaMensagem.Dominio.Interfaces.Repositorios
{
    public interface IHistoricoDisparoRepository
    {
        Task Incluir(HistoricoDisparo historicoDisparo);
        Task<HistoricoDisparo?> ObterPorId(Guid id);
        Task<HistoricoDisparo?> ObterPorWamidMeta(string wamidMeta);
    }
}
