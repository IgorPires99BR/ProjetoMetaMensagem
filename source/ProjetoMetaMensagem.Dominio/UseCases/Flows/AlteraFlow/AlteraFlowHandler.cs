using ProjetoMetaMensagem.Dominio.Common;
using ProjetoMetaMensagem.Dominio.Entidades;
using ProjetoMetaMensagem.Dominio.Interfaces;
using ProjetoMetaMensagem.Dominio.Interfaces.Mediator;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjetoMetaMensagem.Dominio.UseCases.Flows.AlteraFlow
{
    public class AlteraFlowHandler : IRequestHandler<AlteraFlowCommand, Response<AlteraFlowResult>>
    {
        private readonly IUnitOfWork _unitOfWork;

        public AlteraFlowHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<Response<AlteraFlowResult>> Handle(AlteraFlowCommand command)
        {
            var response = new Response<AlteraFlowResult>();

            // 1. Verifica se o Flow realmente existe no banco
            var flowExistente = await _unitOfWork.Flow.ObterPorId(command.Id);
            if (flowExistente == null)
            {
                response.AddErro("Fluxo de conversa não encontrado.");
                return response;
            }

            // 2. Atualiza os dados do cabeçalho
            flowExistente.Nome = command.Nome;
            flowExistente.Descricao = command.Descricao;
            flowExistente.GatilhoInicial = command.GatilhoPalavraChave;
            flowExistente.Ativo = command.Ativo;

            // 3. Monta a nova lista de etapas encadeadas
            var novasEtapas = new List<FlowEtapa>();
            FlowEtapa etapaAnterior = null;
            var passosOrdenados = command.Etapas.OrderBy(e => e.Ordem).ToList();

            for (int i = 0; i < passosOrdenados.Count; i++)
            {
                var dto = passosOrdenados[i];

                var novaEtapa = new FlowEtapa
                {
                    // Mesmo bug do CriaFlowHandler: sem Id explicito, toda etapa nascia
                    // com Guid.Empty e a segunda etapa violava a PK ao salvar.
                    Id = Guid.NewGuid(),
                    FlowId = flowExistente.Id,
                    NomeEtapa = dto.TipoStep,
                    ConteudoLivre = dto.MensagemPergunta,
                    EhEtapaInicial = (i == 0),
                    GatilhoResposta = dto.TipoStep == "Capturar Input" ? "Qualquer_Resposta" : "Avancar",
                    TemplateId = dto.TemplateId
                };

                if (etapaAnterior != null)
                {
                    etapaAnterior.ProximaEtapaId = novaEtapa.Id;
                }

                novasEtapas.Add(novaEtapa);
                etapaAnterior = novaEtapa;
            }

            try
            {
                _unitOfWork.BeginTransaction();
                // 4. Executa as alterações no banco de dados dentro da mesma transação

                // Atualiza o pai
                await _unitOfWork.Flow.Alterar(flowExistente);

                // Remove todas as etapas antigas do banco para limpar o Grafo anterior
                await _unitOfWork.Flow.ExcluirEtapasPorFlowId(flowExistente.Id);

                // Insere a nova arvore de etapas atualizada -- de tras pra frente, mesmo motivo
                // do CriaFlowHandler (ProximaEtapaId e uma FK auto-referenciada em FlowEtapa)
                for (int i = novasEtapas.Count - 1; i >= 0; i--)
                {
                    await _unitOfWork.Flow.IncluirEtapa(novasEtapas[i]);
                }

                // 5. Integração com a Meta
                // Aqui você atualizaria o Flow correspondente na API Cloud do WhatsApp se necessário

                // 7. Retorna o resultado
                var result = new AlteraFlowResult();

                response.AddValue(result);
                _unitOfWork.Commit();
            }
            catch (Exception ex)
            {
                _unitOfWork.Rollback();
                // Qualquer quebra (no banco ou integração) faz o UnitOfWork reverter tudo, mantendo o estado anterior
                response.AddErro($"Erro: {ex.Message}");
            }

            return response;
        }
    }
}


