using ProjetoMetaMensagem.Dominio.Entidades;
using System;

namespace ProjetoMetaMensagem.Dominio.Servicos
{
    // Ponto unico de montagem da CobrancaCliente aberta a cada disparo de um Template com
    // GeraCobranca = true -- usado pelos dois handlers que gravam HistoricoDisparo de template
    // (EnviarMensagemTemplateMetaHandler e ...Lote), pra nao duplicar a regra de snapshot.
    public static class CobrancaClienteFactory
    {
        // Valor/vencimento sem cadastro (ValorFatura/DiaVencimento nulos) caem no zero/hoje --
        // a cobranca ainda assim precisa existir pra nao perder o vinculo com o disparo, mas o
        // valor fica visivelmente errado (0,00) pra quem for conferir, em vez de estourar
        // excecao e derrubar um disparo que ja foi enviado ao cliente.
        public static CobrancaCliente Criar(Contato contato, Guid templateId, Guid historicoDisparoId, DateTime hoje)
        {
            return new CobrancaCliente
            {
                EmpresaId = contato.EmpresaId,
                ContatoId = contato.Id,
                TemplateId = templateId,
                HistoricoDisparoId = historicoDisparoId,
                Valor = contato.ValorFatura ?? 0m,
                DataVencimento = ResolvedorDeVariaveis.CalcularDataVencimento(contato.DiaVencimento, hoje) ?? hoje.Date
            };
        }
    }
}
