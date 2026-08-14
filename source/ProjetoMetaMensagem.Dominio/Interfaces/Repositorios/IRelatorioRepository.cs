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

    public class GastoEmpresaMesDto
    {
        public Guid EmpresaId { get; set; }
        public string NomeEmpresa { get; set; } = string.Empty;
        public int Ano { get; set; }
        public int Mes { get; set; }
        public string Categoria { get; set; } = string.Empty;
        public int Quantidade { get; set; }
        public decimal GastoEstimado { get; set; }
    }

    public class EngajamentoEmpresaDto
    {
        public Guid EmpresaId { get; set; }
        public string NomeEmpresa { get; set; } = string.Empty;
        public int Enviados { get; set; }
        public int Visualizaram { get; set; }
        public int Responderam { get; set; }
    }

    public class PrecoCategoriaDto
    {
        public string Categoria { get; set; } = string.Empty;
        public decimal PrecoUnitario { get; set; }
        public string Moeda { get; set; } = "BRL";
    }

    public interface IRelatorioRepository
    {
        Task<List<RelatorioMensagemDto>> ListarMensagens(Guid empresaId, DateTime? dataInicio, DateTime? dataFim, int pagina, int tamanhoPagina);

        // EmpresaId nulo = todas as empresas (uso restrito a admin, checado no handler).
        Task<List<GastoEmpresaMesDto>> ListarGastoPorEmpresaMes(Guid? empresaId, DateTime? dataInicio, DateTime? dataFim);

        Task<List<EngajamentoEmpresaDto>> ListarEngajamento(Guid? empresaId, DateTime? dataInicio, DateTime? dataFim);

        Task<List<PrecoCategoriaDto>> ListarPrecosCategoria();

        Task AtualizarPrecoCategoria(string categoria, decimal precoUnitario);
    }
}
