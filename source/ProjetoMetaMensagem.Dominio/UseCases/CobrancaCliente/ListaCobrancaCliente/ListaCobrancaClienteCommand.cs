using ProjetoMetaMensagem.Dominio.Common;
using ProjetoMetaMensagem.Dominio.Interfaces.Mediator;
using System;

namespace ProjetoMetaMensagem.Dominio.UseCases.CobrancaCliente.ListaCobrancaCliente
{
    public class ListaCobrancaClienteCommand : IRequest<Response<ListaCobrancaClienteResult>>
    {
        // Escopo sempre vindo do token (nunca do corpo/rota) -- mesmo padrao de
        // ListaAssinaturasCommand: null so pra admin da plataforma (ve todas as empresas).
        public Guid? EmpresaIdSolicitante { get; set; }

        // PENDENTE, PAGA, VENCIDA (filtro calculado: pendente + vencimento no passado) ou
        // CANCELADA. Nulo = todas. E o que o futuro job de recorrencia vai consumir com VENCIDA.
        public string? Status { get; set; }

        // Periodo pela data em que a cobranca foi ENVIADA (DataCriacao = momento do disparo),
        // nao pelo vencimento -- a tela do cliente responde "o que foi cobrado em meu nome
        // neste periodo". Ambos opcionais e inclusivos (DataFim vale o dia inteiro).
        public DateTime? DataInicio { get; set; }
        public DateTime? DataFim { get; set; }
    }
}
