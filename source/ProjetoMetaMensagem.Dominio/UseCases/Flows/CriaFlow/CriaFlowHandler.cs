using ProjetoMetaMensagem.Dominio.Common;
using ProjetoMetaMensagem.Dominio.Entidades;
using ProjetoMetaMensagem.Dominio.Interfaces;
using ProjetoMetaMensagem.Dominio.Interfaces.Mediator;

namespace ProjetoMetaMensagem.Dominio.UseCases.Flows.CriaFlow
{
    public class CriaFlowHandler : IRequestHandler<CriaFlowCommand, Response<CriaFlowResult>>
    {
        private readonly IUnitOfWork _unitOfWork;
        public CriaFlowHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<Response<CriaFlowResult>> Handle(CriaFlowCommand command)
        {
            var response = new Response<CriaFlowResult>();


            Flow flow = new Flow(command);
            var etapasParaSalvar = new List<FlowEtapa>();
            FlowEtapa etapaAnterior = null;

            // Ordena os passos vindos da tela para garantir a lógica sequencial
            var passosOrdenados = command.Etapas.OrderBy(e => e.Ordem).ToList();

            for (int i = 0; i < passosOrdenados.Count; i++)
            {
                var dto = passosOrdenados[i];

                var novaEtapa = new FlowEtapa
                {
                    FlowId = flow.Id,
                    NomeEtapa = dto.TipoStep, // "Mensagem" ou "Capturar Input"
                    ConteudoLivre = dto.MensagemPergunta, // Texto digitado na caixa
                    EhEtapaInicial = (i == 0), // O primeiro item da lista da tela vira a etapa inicial
                    GatilhoResposta = dto.TipoStep == "Capturar Input" ? "Qualquer_Resposta" : "Avancar",
                    TemplateId = dto.TemplateId
                };

                // Se houver uma etapa anterior no loop, atualizamos o ponteiro dela para esta nova
                if (etapaAnterior != null)
                {
                    etapaAnterior.ProximaEtapaId = novaEtapa.Id;
                }

                etapasParaSalvar.Add(novaEtapa);
                etapaAnterior = novaEtapa; // Atualiza a referência para o próximo loop
            }

            try
            {
                _unitOfWork.BeginTransaction();
                // 3. Salva o Flow Pai
                await _unitOfWork.Flow.Incluir(flow);

                // 4. Salva os filhos (Etapas) já encadeadas
                foreach (var etapa in etapasParaSalvar)
                {
                    // Nota: Certifique-se que seu UnitOfWork dê acesso ao método de incluir etapas
                    await _unitOfWork.Flow.IncluirEtapa(etapa);
                }

                // 7. Retorno de sucesso
                var result = new CriaFlowResult();

                response.AddValue(result);
                _unitOfWork.Commit();
            }
            catch (Exception ex)
            {
                _unitOfWork.Rollback();
                // Caso ocorra qualquer erro no processo do banco ou da Meta, a transação sofre rollback
                // e o banco não fica com dados fragmentados.
                response.AddErro($"Erro: {ex.Message}");
            }

            return response;
        }
    }
}


