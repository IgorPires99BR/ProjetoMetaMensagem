using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ProjetoMetaMensagem.Dominio.Interfaces.Repositorios
{
    public class RelatorioMensagemDto
    {
        public string Direcao { get; set; } = string.Empty; // "Enviada" ou "Recebida"
        public string NumeroOrigem { get; set; } = string.Empty;
        public string NumeroDestino { get; set; } = string.Empty;
        public string Conteudo { get; set; } = string.Empty;
        public DateTime DataHora { get; set; }
        public string? Status { get; set; }
    }

    public interface IRelatorioRepository
    {
        Task<List<RelatorioMensagemDto>> ListarMensagens(Guid empresaId, DateTime? dataInicio, DateTime? dataFim, int pagina, int tamanhoPagina);
    }
}
