using ProjetoMetaMensagem.Dominio.Common;
using ProjetoMetaMensagem.Dominio.Interfaces.Mediator;
using System;

namespace ProjetoMetaMensagem.Dominio.UseCases.Empresa.SalvaDadosBancarios
{
    // A subconta Asaas nao passa por aqui: nasce em CriaSubcontaAsaas e nao pode ser trocada
    // pela tela (o walletId so vem da resposta do Asaas).
    public class SalvaDadosBancariosCommand : IRequest<Response<SalvaDadosBancariosResult>>
    {
        public Guid EmpresaId { get; set; }
        public bool CobrancaPixAtiva { get; set; }

        // So vale quando quem salva e admin da plataforma; para os demais o handler mantem a
        // comissao gravada -- senao o admin da empresa poderia zerar a propria comissao.
        public string? ComissaoTipo { get; set; }
        public decimal? ComissaoValor { get; set; }

        // Vem do token, nunca do corpo (preenchido pelo controller).
        public bool SolicitanteEhAdminDaPlataforma { get; set; }
    }
}
